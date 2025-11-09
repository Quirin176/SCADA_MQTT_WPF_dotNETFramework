using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgUnsubscribeEventArgs : EventArgs
    {
        private ushort messageId;

        private string[] topics;

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

        public string[] Topics
        {
            get
            {
                return topics;
            }
            internal set
            {
                topics = value;
            }
        }

        public MQTTMsgUnsubscribeEventArgs(ushort messageId, string[] topics)
        {
            this.messageId = messageId;
            this.topics = topics;
        }
    }
}
