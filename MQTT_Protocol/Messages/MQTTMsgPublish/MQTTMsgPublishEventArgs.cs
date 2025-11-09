using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgPublishEventArgs : EventArgs
    {
        private string topic;

        private byte[] message;

        private bool dupFlag;

        private byte qosLevel;

        private bool retain;

        public string Topic
        {
            get
            {
                return topic;
            }
            internal set
            {
                topic = value;
            }
        }

        public byte[] Message
        {
            get
            {
                return message;
            }
            internal set
            {
                message = value;
            }
        }

        public bool DupFlag
        {
            get
            {
                return dupFlag;
            }
            set
            {
                dupFlag = value;
            }
        }

        public byte QosLevel
        {
            get
            {
                return qosLevel;
            }
            internal set
            {
                qosLevel = value;
            }
        }

        public bool Retain
        {
            get
            {
                return retain;
            }
            internal set
            {
                retain = value;
            }
        }

        public MQTTMsgPublishEventArgs(string topic, byte[] message, bool dupFlag, byte qosLevel, bool retain)
        {
            this.topic = topic;
            this.message = message;
            this.dupFlag = dupFlag;
            this.qosLevel = qosLevel;
            this.retain = retain;
        }
    }
}
