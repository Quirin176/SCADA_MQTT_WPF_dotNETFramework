using MQTT_Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Internal
{
    public class MsgInternalEvent : InternalEvent
    {
        protected MQTTMsgBase msg;

        public MQTTMsgBase Message
        {
            get
            {
                return msg;
            }
            set
            {
                msg = value;
            }
        }

        public MsgInternalEvent(MQTTMsgBase msg)
        {
            this.msg = msg;
        }
    }
}
