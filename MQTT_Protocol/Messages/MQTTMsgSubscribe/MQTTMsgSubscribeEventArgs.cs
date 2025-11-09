using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgSubscribeEventArgs : EventArgs
    {
        private ushort messageId;

        private string[] topics;

        private byte[] qosLevels;

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

        public byte[] QoSLevels
        {
            get
            {
                return qosLevels;
            }
            internal set
            {
                qosLevels = value;
            }
        }

        public MQTTMsgSubscribeEventArgs(ushort messageId, string[] topics, byte[] qosLevels)
        {
            this.messageId = messageId;
            this.topics = topics;
            this.qosLevels = qosLevels;
        }
    }
}
