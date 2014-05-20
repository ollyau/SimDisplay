using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimDisplay {
    class Program {
        static void Main(string[] args) {
            // write info
            Console.WriteLine("SimDisplay (" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + ")\r\nBy Orion Lyau\r\n");

            // connect to SimConnect
            SimConnectInstance sc = SimConnectInstance.Instance;
            sc.Connect();

            // instantiate SimDisplay class
            SimDisplay sd;
            if (args.Length == 0) {
                sd = new SimDisplay();
            }
            else {
                sd = new SimDisplay(args[0]);
            }

            // exit message
            Console.WriteLine("Press enter to close.\r\n");
            Console.ReadLine();
            if (sc.Connected) {
                sc.Disconnect();
            }
        }
    }
}
