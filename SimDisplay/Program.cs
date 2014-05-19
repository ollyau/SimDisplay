using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimDisplay {
    class Program {
        static void Main(string[] args) {
            SimConnectInstance sc = new SimConnectInstance();
            sc.Connect();
            sc = null;
        }
    }
}
