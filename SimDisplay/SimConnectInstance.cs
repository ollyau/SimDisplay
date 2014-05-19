using BeatlesBlog.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimDisplay {
    class SimConnectInstance {
        static public string AssemblyLoadDirectory {
            get {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        bool InvalidFileNameChars(string input) {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) {
                if (input.Contains(c)) { return true; }
            }
            return false;
        }

        bool InvalidPathChars(string input) {
            foreach (char c in System.IO.Path.GetInvalidPathChars()) {
                if (input.Contains(c)) { return true; }
            }
            return false;
        }

        // Private fields
        const string appName = "SimDisplay";
        string settingsFilePath;
        SimConnect sc = null;
        XmlSerializer serializer;
        Settings s;
        System.Threading.AutoResetEvent exitEvent = new System.Threading.AutoResetEvent(false);

        /// <summary>
        /// Default constructor.  Instantiates the class and hooks SimConnect event handlers.
        /// </summary>
        public SimConnectInstance(string settingsFilename = "SimDisplay.xml") {
            if (!System.IO.Path.IsPathRooted(settingsFilename)) {
                // assume it's relative to the exe directory
                if (InvalidFileNameChars(settingsFilename)) {
                    throw new ArgumentException("Invalid filename.");
                }
                else {
                    settingsFilePath = System.IO.Path.Combine(AssemblyLoadDirectory, settingsFilename);
                }
            }
            else {
                // otherwise it's a full path
                if (InvalidPathChars(settingsFilename)) {
                    throw new ArgumentException("Invalid file path.");
                }
                else {
                    settingsFilePath = settingsFilename;
                }
            }

            // Instantiate the SimConnect class
            sc = new SimConnect(null);

            // hook needed events
            sc.OnRecvOpen += new SimConnect.RecvOpenEventHandler(sc_OnRecvOpen);
            sc.OnRecvException += new SimConnect.RecvExceptionEventHandler(sc_OnRecvException);
            sc.OnRecvQuit += new SimConnect.RecvQuitEventHandler(sc_OnRecvQuit);

            sc.OnRecvReservedKey += sc_OnRecvReservedKey;
            sc.OnRecvAssignedObjectId += sc_OnRecvAssignedObjectId;

            // Write info
            AddOutput(appName + " (" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + ")\r\nBy Orion Lyau\r\n");
        }

        ~SimConnectInstance() {
            sc = null;
        }

        private void AddOutput(string text) {
            Console.WriteLine(text);
        }

        public void Connect() {
            try {
                sc.Open(appName);
            }
            catch (SimConnect.SimConnectException) {
                Console.WriteLine("Local connection failed.  Press any key to exit.");
                Console.ReadKey();
                return;
            }
            exitEvent.WaitOne();
        }

        public void Disconnect() {
            AddOutput("Disconnecting.");

            sc.Close();

            // Save settings
            using (System.IO.Stream writer = new System.IO.FileStream(settingsFilePath, System.IO.FileMode.Create)) {
                serializer.Serialize(writer, s);
            }

            exitEvent.Set();
        }

        void sc_OnRecvOpen(SimConnect sender, BeatlesBlog.SimConnect.SIMCONNECT_RECV_OPEN data) {
            AddOutput("Connected to " + data.szApplicationName +
                "\r\n    Simulator Version:\t" + data.dwApplicationVersionMajor + "." + data.dwApplicationVersionMinor + "." + data.dwApplicationBuildMajor + "." + data.dwApplicationBuildMinor +
                "\r\n    SimConnect Version:\t" + data.dwSimConnectVersionMajor + "." + data.dwSimConnectVersionMinor + "." + data.dwSimConnectBuildMajor + "." + data.dwSimConnectBuildMinor +
                "\r\n");

            sc.Text(SIMCONNECT_TEXT_TYPE.PRINT_WHITE, 5.0f, Requests.DisplayText, appName + " is connected to " + Encoding.UTF8.GetString(Encoding.Default.GetBytes(data.szApplicationName)));

            // load settings
            serializer = new XmlSerializer(typeof(Settings));
            if (System.IO.File.Exists(settingsFilePath)) {
#if !DEBUG
                try {
#endif
                using (System.IO.Stream reader = new System.IO.FileStream(settingsFilePath, System.IO.FileMode.Open)) {
                    s = (Settings)serializer.Deserialize(reader);
                }
#if !DEBUG
                }
                catch (Exception ex) {
                    Console.WriteLine("Unable to load settings XML file:\r\n\r\n{0}\r\n\r\n{1}", System.IO.Path.Combine(AssemblyLoadDirectory, settingsFilename), ex.Message);
                    s = new Settings();
                }
#endif
            }
            else {
                s = new Settings();
                s.Objects.Add(new SimObject("veh_carrier01_high_detail_sm", 37.793322, -122.335726, 0, 0, 0, 0, true, 0));
                s.Objects.Add(new SimObject("VEH_water_yacht_280ft_sm", 37.787659, -122.333902, 0, 0, 0, 0, true, 0));
                s.Objects.Add(new SimObject("VEH_Air_PickupUS_Grey_sm", 37.790607, -122.330697, 0, 0, 0, 0, true, 0));
                s.Objects.Add(new SimObject("Discovery Spaceshuttle", 37.790486, -122.328211, 0, 0, 0, 0, true, 0));
                s.Objects.Add(new SimObject("Robinson R22", 37.790314, -122.330437, 0, 0, 0, 0, true, 0));
                s.Objects.Add(new SimObject("ANI_GiraffeWalk_Mature_sm", 37.790338, -122.329498, 0, 0, 0, 0, true, 0));
                s.Objects.Add(new SimObject("Cessna Skyhawk 172SP Paint1", 37.790615, -122.330056, 0, 0, 0, 0, true, 0));
            }

            // load objects into sim
            foreach (SimObject obj in s.Objects) {
                CreateSimulatedObject(obj);
            }
        }

        void sc_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data) {
            AddOutput("OnRecvException: " + data.dwException.ToString() + " (" + Enum.GetName(typeof(SIMCONNECT_EXCEPTION), data.dwException) + ")" + "  " + data.dwSendID.ToString() + "  " + data.dwIndex.ToString());
            sc.Text(SIMCONNECT_TEXT_TYPE.PRINT_WHITE, 10.0f, Requests.DisplayText, appName + " SimConnect Exception: " + data.dwException.ToString() + " (" + Enum.GetName(typeof(SIMCONNECT_EXCEPTION), data.dwException) + ")");
        }

        void sc_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data) {
            AddOutput("OnRecvQuit\tSimulator has closed.");
            Disconnect();
        }

        void sc_OnRecvReservedKey(SimConnect sender, SIMCONNECT_RECV_RESERVED_KEY data) {
            AddOutput("Key reserved: " + data.szReservedKey);
        }

        void sc_OnRecvAssignedObjectId(SimConnect sender, SIMCONNECT_RECV_ASSIGNED_OBJECT_ID data) {
            switch ((Requests)data.dwRequestID) {
                case Requests.CreateAI:
                    AddOutput("OnRecvAssignedObjectId: " + data.dwObjectID + " (CreateAI)");
                    break;
                default:
                    AddOutput("OnRecvAssignedObjectId: " + data.dwObjectID + " (unknown request)");
                    break;
            }
        }

        /// <summary>
        /// Creates a SimObject using AICreateSimulatedObject.
        /// </summary>
        /// <param name="title">SimObject class to represent the title and initposition.</param>
        private void CreateSimulatedObject(SimObject obj) {
            AddOutput("CreateSimulatedObject:");
            foreach (System.Reflection.FieldInfo x in obj.GetType().GetFields()) {
                AddOutput("\t" + x.Name + ": " + x.GetValue(obj));
            }
            sc.AICreateSimulatedObject(obj.Title, new SIMCONNECT_DATA_INITPOSITION(obj.Latitude, obj.Longitude, obj.Altitude, obj.Pitch, obj.Bank, obj.Heading, obj.OnGround, obj.Airspeed), Requests.CreateAI);
        }
    }
}
