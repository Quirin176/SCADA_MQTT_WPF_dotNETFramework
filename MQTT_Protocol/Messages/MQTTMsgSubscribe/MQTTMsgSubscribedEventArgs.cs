using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgSubscribedEventArgs : EventArgs
    {
        private ushort messageId;

        private byte[] grantedQosLevels;

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

        public byte[] GrantedQoSLevels
        {
            get
            {
                return grantedQosLevels;
            }
            internal set
            {
                grantedQosLevels = value;
            }
        }

        public MQTTMsgSubscribedEventArgs(ushort messageId, byte[] grantedQosLevels)
        {
            this.messageId = messageId;
            this.grantedQosLevels = grantedQosLevels;
        }
    }
}
