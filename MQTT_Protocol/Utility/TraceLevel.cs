using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Utility
{
    public enum TraceLevel
    {
        Error = 1,
        Warning = 2,
        Information = 4,
        Verbose = 15,
        Frame = 16,
        Queuing = 32
    }
}
