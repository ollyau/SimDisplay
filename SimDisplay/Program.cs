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
            SimDisplay sd;
            if (args.Length == 0) {
                sd = new SimDisplay();
            }
            else {
                sd = new SimDisplay(args[0]);
            }
            Console.WriteLine("Press enter to close.\r\n");
            Console.ReadLine();
            SimConnectInstance.Instance.Disconnect();
        }
    }
}
