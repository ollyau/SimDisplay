using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimDisplay {
    public class SimObject {
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

        public SimObject() { }
        
        public SimObject(string title, double latitude, double longitude, double altitude, double pitch, double bank, double heading, bool onGround, uint airspeed) {
            this.Title = title;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
            this.Pitch = pitch;
            this.Bank = bank;
            this.Heading = heading;
            this.OnGround = onGround;
            this.Airspeed = airspeed;
        }
    }
}
