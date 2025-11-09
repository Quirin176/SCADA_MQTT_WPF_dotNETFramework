using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgContext
    {
        public MQTTMsgBase Message { get; set; }
        public MQTTMsgState State { get; set; }

        public MQTTMsgFlow Flow { get; set; }

        public int Timestamp { get; set; }

        public int Attempt { get; set; }

        public string Key => string.Concat(Flow, "_", Message.MessageId);

    }
}
