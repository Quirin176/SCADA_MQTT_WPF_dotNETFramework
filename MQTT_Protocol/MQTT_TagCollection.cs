using MQTT_Protocol.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol
{
    public static class MQTT_TagCollection
    {
        private static Dictionary<string, Tag> _Tags = new Dictionary<string, Tag>();

        public static Dictionary<string, Tag> Tags
        {
            get { return _Tags; }
            set { _Tags = value; }
        }
    }
}
