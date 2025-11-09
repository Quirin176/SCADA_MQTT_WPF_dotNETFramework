using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgUnsubscribedEventArgs : EventArgs
    {
        private ushort messageId;

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

        public MQTTMsgUnsubscribedEventArgs(ushort messageId)
        {
            this.messageId = messageId;
        }
    }
}
