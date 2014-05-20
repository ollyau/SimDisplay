using BeatlesBlog.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimDisplay {
    public enum LoadTypes {
        Immediate,
        Conditional
    }

    public struct Waypoint {
        [XmlAttribute(AttributeName = "Altitude")]
        public double Altitude;
        [XmlAttribute(AttributeName = "Flags")]
        public SIMCONNECT_WAYPOINT_FLAGS Flags;
        [XmlAttribute(AttributeName = "Latitude")]
        public double Latitude;
        [XmlAttribute(AttributeName = "Longitude")]
        public double Longitude;
        [XmlAttribute(AttributeName = "SpeedOrThrottle")]
        public double SpeedOrThrottle;

        public Waypoint(double Latitude, double Longitude, double Altitude, SIMCONNECT_WAYPOINT_FLAGS Flags, double SpeedOrThrottle) {
            this.Altitude = Altitude;
            this.Flags = Flags;
            this.Latitude = Latitude;
            this.Longitude = Longitude;
            this.SpeedOrThrottle = SpeedOrThrottle;
        }
    }

    public class SimObject {
        [XmlIgnore()]
        public uint ObjectId = 0;

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

        [XmlAttribute(AttributeName = "LoadType")]
        public LoadTypes LoadType;

        public List<Waypoint> WaypointList;

        public SimObject() { }

        public SimObject(string title, double latitude, double longitude, double altitude, double pitch, double bank, double heading, bool onGround, uint airspeed, bool simDisabled = false, LoadTypes loadType = LoadTypes.Immediate)
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
            this.LoadType = loadType;
        }
    }
}
