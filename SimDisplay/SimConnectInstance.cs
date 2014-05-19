using BeatlesBlog.SimConnect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimDisplay {
    class SimConnectInstance {
        // instance of the singleton class
        private static readonly Lazy<SimConnectInstance> sci = new Lazy<SimConnectInstance>(() => new SimConnectInstance());

        // static property to get the instance
        public static SimConnectInstance Instance { get { return sci.Value; } }

        public bool IsConnected = false;

        // Private fields
        const string appName = "SimDisplay";
        SimConnect sc = null;
        List<SimObject> Objects;

        private SimConnectInstance() {
            // instantiate SimConnect
            sc = new SimConnect(null);

            // hook events
            sc.OnRecvOpen += sc_OnRecvOpen;
            sc.OnRecvException += sc_OnRecvException;
            sc.OnRecvQuit += sc_OnRecvQuit;
            sc.OnRecvAssignedObjectId += sc_OnRecvAssignedObjectId;
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
                Console.WriteLine("Local connection failed.");
                return;
            }
        }

        public void Disconnect() {
            AddOutput("Disconnecting.");
            sc.Close();
            IsConnected = false;
        }

        // SimConnect event handlers

        private void sc_OnRecvOpen(SimConnect sender, BeatlesBlog.SimConnect.SIMCONNECT_RECV_OPEN data) {
            // inform user
            AddOutput("Connected to " + data.szApplicationName +
                "\r\n    Simulator Version:\t" + data.dwApplicationVersionMajor + "." + data.dwApplicationVersionMinor + "." + data.dwApplicationBuildMajor + "." + data.dwApplicationBuildMinor +
                "\r\n    SimConnect Version:\t" + data.dwSimConnectVersionMajor + "." + data.dwSimConnectVersionMinor + "." + data.dwSimConnectBuildMajor + "." + data.dwSimConnectBuildMinor +
                "\r\n");

            sc.Text(SIMCONNECT_TEXT_TYPE.PRINT_WHITE, 5.0f, Requests.DisplayText, appName + " is connected to " + Encoding.UTF8.GetString(Encoding.Default.GetBytes(data.szApplicationName)));

            IsConnected = true;

            // initialize list of SimObjects
            Objects = new List<SimObject>();
        }

        private void sc_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data) {
            AddOutput("OnRecvException: " + data.dwException.ToString() + " (" + Enum.GetName(typeof(SIMCONNECT_EXCEPTION), data.dwException) + ")" + "  " + data.dwSendID.ToString() + "  " + data.dwIndex.ToString());
            sc.Text(SIMCONNECT_TEXT_TYPE.PRINT_WHITE, 10.0f, Requests.DisplayText, appName + " SimConnect Exception: " + data.dwException.ToString() + " (" + Enum.GetName(typeof(SIMCONNECT_EXCEPTION), data.dwException) + ")");
        }

        private void sc_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data) {
            AddOutput("OnRecvQuit\tSimulator has closed.");
            Disconnect();
        }

        private void sc_OnRecvAssignedObjectId(SimConnect sender, SIMCONNECT_RECV_ASSIGNED_OBJECT_ID data) {
            switch ((Requests)data.dwRequestID) {
                case Requests.AICreate:
                    AddOutput("OnRecvAssignedObjectId: " + data.dwObjectID + " (CreateAI)");
                    break;
                default:
                    AddOutput("OnRecvAssignedObjectId: " + data.dwObjectID + " (unknown request)");
                    break;
            }
        }

        // public functions

        public void AICreateSimulatedObject(string szContainerTitle, double Latitude, double Longitude, double Altitude, double Pitch, double Bank, double Heading, bool OnGround, uint Airspeed) {
            if (IsConnected) {
                sc.AICreateSimulatedObject(szContainerTitle, new SIMCONNECT_DATA_INITPOSITION(Latitude, Longitude, Altitude, Pitch, Bank, Heading, OnGround, Airspeed), Requests.AICreate);
            }
        }

        public void AIReleaseControl(uint dwObjectID) {
            if (IsConnected) {
                sc.AIReleaseControl(dwObjectID, Requests.AIRelease);
            }
        }

        public void AIRemoveObject(uint dwObjectID) {
            if (IsConnected) {
                sc.AIRemoveObject(dwObjectID, Requests.AIRemove);
            }
        }

        public void SetDataOnSimObject(uint dwObjectID, object pDataSet) {
            if (IsConnected) {
                sc.SetDataOnSimObject(dwObjectID, pDataSet);
            }
        }

        public void SetDataOnSimObject(uint dwObjectID, IEnumerable DataSet) {
            if (IsConnected) {
                sc.SetDataOnSimObject(dwObjectID, DataSet);
            }
        }
    }
}
