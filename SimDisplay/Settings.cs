using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimDisplay {
    [XmlRoot(ElementName = "SimDisplay")]
    public class Settings {

        [XmlElement(ElementName = "SimObject")]
        public List<SimObject> Objects;

        public Settings() {
            Objects = new List<SimObject>();
        }
    }
}
