using BeatlesBlog.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimDisplay {
    [DataStruct()]
    public struct SimDataStruct1 {
        [DataItem("SIM DISABLED", "Bool")]
        public bool SimDisabled;
    }

    enum Requests {
        DisplayText,
        AIRemove,
        AIRelease,
        AICreate = 0x01000000,
        AIRequest = 0x02000000
    }

    enum Events {
        AddObject,
        RemoveObject
    }
}
