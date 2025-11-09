using MQTT_Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Internal
{
    public class MsgPublishedInternalEvent : MsgInternalEvent
    {
        private bool isPublished;

        public bool IsPublished
        {
            get
            {
                return isPublished;
            }
            internal set
            {
                isPublished = value;
            }
        }

        public MsgPublishedInternalEvent(MQTTMsgBase msg, bool isPublished)
            : base(msg)
        {
            this.isPublished = isPublished;
        }
    }
}
