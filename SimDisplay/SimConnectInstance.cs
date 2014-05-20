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

        public bool Connected = false;

        // Private fields
        const string appName = "SimDisplay";
        SimConnect sc = null;
        List<SimObject> ObjectsToAdd;
        Dictionary<uint, SimObject> ObjectsInSim;
        uint ObjectIndex = 0;
        const uint MaxSimObjectsAllowed = 10000;

        private SimConnectInstance() {
            // instantiate SimConnect
            sc = new SimConnect(null);

            // hook events
            sc.OnRecvOpen += sc_OnRecvOpen;
            sc.OnRecvException += sc_OnRecvException;
            sc.OnRecvQuit += sc_OnRecvQuit;

            sc.OnRecvEventObjectAddremove += sc_OnRecvEventObjectAddremove;
            sc.OnRecvAssignedObjectId += sc_OnRecvAssignedObjectId;
            //sc.OnRecvSimobjectData += sc_OnRecvSimobjectData;

            // initialize list and dictionary of SimObjects
            ObjectsToAdd = new List<SimObject>();
            ObjectsInSim = new Dictionary<uint, SimObject>();
        }

        ~SimConnectInstance() {
            sc = null;
            ObjectsToAdd = null;
            ObjectsInSim = null;
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
            Connected = false;
        }

        // SimConnect event handlers

        private void sc_OnRecvOpen(SimConnect sender, BeatlesBlog.SimConnect.SIMCONNECT_RECV_OPEN data) {
            // inform user
            AddOutput("Connected to " + data.szApplicationName +
                "\r\n    Simulator Version:\t" + data.dwApplicationVersionMajor + "." + data.dwApplicationVersionMinor + "." + data.dwApplicationBuildMajor + "." + data.dwApplicationBuildMinor +
                "\r\n    SimConnect Version:\t" + data.dwSimConnectVersionMajor + "." + data.dwSimConnectVersionMinor + "." + data.dwSimConnectBuildMajor + "." + data.dwSimConnectBuildMinor +
                "\r\n");

            sc.Text(SIMCONNECT_TEXT_TYPE.PRINT_WHITE, 5.0f, Requests.DisplayText, appName + " is connected to " + Encoding.UTF8.GetString(Encoding.Default.GetBytes(data.szApplicationName)));

            Connected = true;

            // Subscribe to events
            sc.SubscribeToSystemEvent(Events.AddObject, "ObjectAdded");
            sc.SubscribeToSystemEvent(Events.RemoveObject, "ObjectRemoved");
        }

        private void sc_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data) {
            AddOutput("OnRecvException: " + data.dwException.ToString() + " (" + Enum.GetName(typeof(SIMCONNECT_EXCEPTION), data.dwException) + ")" + "  " + data.dwSendID.ToString() + "  " + data.dwIndex.ToString());
            sc.Text(SIMCONNECT_TEXT_TYPE.PRINT_WHITE, 10.0f, Requests.DisplayText, appName + " SimConnect Exception: " + data.dwException.ToString() + " (" + Enum.GetName(typeof(SIMCONNECT_EXCEPTION), data.dwException) + ")");
        }

        private void sc_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data) {
            AddOutput("OnRecvQuit\tSimulator has closed.");
            Disconnect();
        }

        //void sc_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data) {
        //    throw new NotImplementedException();
        //}

        void sc_OnRecvEventObjectAddremove(SimConnect sender, SIMCONNECT_RECV_EVENT_OBJECT_ADDREMOVE data) {
            switch ((Events)data.uEventID) {
                case Events.AddObject:
                    // let the user know
                    AddOutput("AddObject:\t\t" + data.dwData + " SIMCONNECT_SIMOBJECT_TYPE: " + Enum.GetName(typeof(SIMCONNECT_SIMOBJECT_TYPE), data.eObjType));
                    break;
                case Events.RemoveObject:
                    if (ObjectsInSim.Keys.Contains(data.dwData)) {
                        // if one we made is removed, let the user know
                        AddOutput("RemoveObject:\t\t" + data.dwData + " (created by client)");

                        // and remove it from the list
                        ObjectsInSim.Remove(ObjectsInSim.Single(x => x.Value.ObjectId == data.dwData).Key);
                    }
                    else {
                        // just inform user that something was removed
                        AddOutput("RemoveObject:\t\t" + data.dwData + " (unknown)");
                    }
                    break;
            }
        }

        private void sc_OnRecvAssignedObjectId(SimConnect sender, SIMCONNECT_RECV_ASSIGNED_OBJECT_ID data) {
            if (data.dwRequestID >= (uint)Requests.AICreateBase && data.dwRequestID < (uint)Requests.AICreateBase + MaxSimObjectsAllowed) {
                // get index and set object id on associated simobject instance
                uint objectIndex = data.dwRequestID - (uint)Requests.AICreateBase;
                SimObject obj = ObjectsInSim[objectIndex];
                obj.ObjectId = data.dwObjectID;

                // inform user
                AddOutput("AssignedObjectId:\t" + data.dwObjectID + " objectIndex: " + objectIndex);

                // optionally disable sim
                if (obj.SimDisabled) {
                    SetSimDisable(data.dwObjectID, obj.SimDisabled);
                }

                // send waypoints if they exist
                if (obj.WaypointList != null && obj.WaypointList.Count > 0) {
                    SendWaypointListToSimObject(data.dwObjectID, obj.WaypointList);
                }
            }
        }

        // public functions

        public void AICreateSimulatedObject(SimObject obj) {
            ObjectsInSim.Add(++ObjectIndex, obj);
            sc.AICreateSimulatedObject(obj.Title, new SIMCONNECT_DATA_INITPOSITION(obj.Latitude, obj.Longitude, obj.Altitude, obj.Pitch, obj.Bank, obj.Heading, obj.OnGround, obj.Airspeed), (Requests)((int)Requests.AICreateBase + ObjectIndex));
        }

        public void AIReleaseControl(uint dwObjectID) {
            sc.AIReleaseControl(dwObjectID, Requests.AIRelease);
        }

        public void AIRemoveObject(uint dwObjectID) {
            sc.AIRemoveObject(dwObjectID, Requests.AIRemove);
        }

        public void SendWaypointListToSimObject(uint dwObjectID, List<Waypoint> DataSet) {
            List<SIMCONNECT_DATA_WAYPOINT> waypointList = new List<SIMCONNECT_DATA_WAYPOINT>();
            foreach (Waypoint wp in DataSet) {
                waypointList.Add(new SIMCONNECT_DATA_WAYPOINT(wp.Latitude, wp.Longitude, wp.Altitude, wp.Flags, wp.SpeedOrThrottle));
            }
            sc.SetDataOnSimObject(dwObjectID, waypointList);
        }

        public void SetSimDisable(uint dwObjectID, bool simDisabled) {
            SimDisabledStruct ds1 = new SimDisabledStruct { SimDisabled = simDisabled };
            sc.SetDataOnSimObject(dwObjectID, ds1);
        }
    }
}
