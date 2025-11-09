using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgConnectEventArgs : EventArgs
    {
        public MQTTMsgConnect Message { get; private set; }

        public MQTTMsgConnectEventArgs(MQTTMsgConnect connect)
        {
            Message = connect;
        }
    }
}
