using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgPublishedEventArgs : EventArgs
    {
        private ushort messageId;

        private bool isPublished;

        public ushort MessageId
        {
            get
            {
                return messageId;
            }
            internal set
            {
                messageId = value;
            }
        }

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

        public MQTTMsgPublishedEventArgs(ushort messageId)
            : this(messageId, isPublished: true)
        {

        }

        public MQTTMsgPublishedEventArgs(ushort messageId, bool isPublished)
        {
            this.messageId = messageId;
            this.isPublished = isPublished;
        }
    }
}
