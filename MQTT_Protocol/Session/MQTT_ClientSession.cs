using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Session
{
    public class MQTT_ClientSession : MQTT_Session
    {
        public MQTT_ClientSession(string clientId)
            : base(clientId)
        {
        }
    }
}
