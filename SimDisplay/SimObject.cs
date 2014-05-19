using BeatlesBlog.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimDisplay {
    public class SimObject {
        [XmlIgnore()]
        private uint ObjectId;

        [XmlIgnore()]
        private SimConnectInstance sci;

        [XmlAttribute(AttributeName = "Title")]
        public string Title;

        [XmlAttribute(AttributeName = "Latitude")]
        public double Latitude;

        [XmlAttribute(AttributeName = "Longitude")]
        public double Longitude;

        [XmlAttribute(AttributeName = "Altitude")]
        public double Altitude;

        [XmlAttribute(AttributeName = "Pitch")]
        public double Pitch;

        [XmlAttribute(AttributeName = "Bank")]
        public double Bank;

        [XmlAttribute(AttributeName = "Heading")]
        public double Heading;

        [XmlAttribute(AttributeName = "OnGround")]
        public bool OnGround;

        [XmlAttribute(AttributeName = "Airspeed")]
        public uint Airspeed;

        [XmlAttribute(AttributeName = "SimDisabled")]
        public bool SimDisabled;

        public List<SIMCONNECT_DATA_WAYPOINT> Waypoints { get; set; }

        public SimObject() {
            sci = SimConnectInstance.Instance;
        }

        public SimObject(string title, double latitude, double longitude, double altitude, double pitch, double bank, double heading, bool onGround, uint airspeed, bool simDisabled = false)
            : this() {
            this.Title = title;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
            this.Pitch = pitch;
            this.Bank = bank;
            this.Heading = heading;
            this.OnGround = onGround;
            this.Airspeed = airspeed;
            this.SimDisabled = simDisabled;
        }

        public void CreateSimulatedObject() {
            if (ObjectId == 0) {
                sci.AICreateSimulatedObject(Title, Latitude, Longitude, Altitude, Pitch, Bank, Heading, OnGround, Airspeed);
            }
        }

        public void RemoveObject() {
            if (ObjectId != 0) {
                sci.AIRemoveObject(ObjectId);
            }
        }

        public void SetWaypoints() {
            if (ObjectId == 0 && Waypoints != null) {
                sci.SetDataOnSimObject(ObjectId, Waypoints);
            }
        }

        public void ReleaseControl() {
            if (ObjectId != 0) {
                sci.AIReleaseControl(ObjectId);
            }
        }
    }
}
