using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public abstract class MQTTMsgBase
    {
        internal const byte MSG_TYPE_MASK = 240;

        internal const byte MSG_TYPE_OFFSET = 4;

        internal const byte MSG_TYPE_SIZE = 4;

        internal const byte MSG_FLAG_BITS_MASK = 15;

        internal const byte MSG_FLAG_BITS_OFFSET = 0;

        internal const byte MSG_FLAG_BITS_SIZE = 4;

        internal const byte DUP_FLAG_MASK = 8;

        internal const byte DUP_FLAG_OFFSET = 3;

        internal const byte DUP_FLAG_SIZE = 1;

        internal const byte QOS_LEVEL_MASK = 6;

        internal const byte QOS_LEVEL_OFFSET = 1;

        internal const byte QOS_LEVEL_SIZE = 2;

        internal const byte RETAIN_FLAG_MASK = 1;

        internal const byte RETAIN_FLAG_OFFSET = 0;

        internal const byte RETAIN_FLAG_SIZE = 1;

        internal const byte MQTT_MSG_CONNECT_TYPE = 1;

        internal const byte MQTT_MSG_CONNACK_TYPE = 2;

        internal const byte MQTT_MSG_PUBLISH_TYPE = 3;

        internal const byte MQTT_MSG_PUBACK_TYPE = 4;

        internal const byte MQTT_MSG_PUBREC_TYPE = 5;

        internal const byte MQTT_MSG_PUBREL_TYPE = 6;

        internal const byte MQTT_MSG_PUBCOMP_TYPE = 7;

        internal const byte MQTT_MSG_SUBSCRIBE_TYPE = 8;

        internal const byte MQTT_MSG_SUBACK_TYPE = 9;

        internal const byte MQTT_MSG_UNSUBSCRIBE_TYPE = 10;

        internal const byte MQTT_MSG_UNSUBACK_TYPE = 11;

        internal const byte MQTT_MSG_PINGREQ_TYPE = 12;

        internal const byte MQTT_MSG_PINGRESP_TYPE = 13;

        internal const byte MQTT_MSG_DISCONNECT_TYPE = 14;

        internal const byte MQTT_MSG_CONNECT_FLAG_BITS = 0;

        internal const byte MQTT_MSG_CONNACK_FLAG_BITS = 0;

        internal const byte MQTT_MSG_PUBLISH_FLAG_BITS = 0;

        internal const byte MQTT_MSG_PUBACK_FLAG_BITS = 0;

        internal const byte MQTT_MSG_PUBREC_FLAG_BITS = 0;

        internal const byte MQTT_MSG_PUBREL_FLAG_BITS = 2;

        internal const byte MQTT_MSG_PUBCOMP_FLAG_BITS = 0;

        internal const byte MQTT_MSG_SUBSCRIBE_FLAG_BITS = 2;

        internal const byte MQTT_MSG_SUBACK_FLAG_BITS = 0;

        internal const byte MQTT_MSG_UNSUBSCRIBE_FLAG_BITS = 2;

        internal const byte MQTT_MSG_UNSUBACK_FLAG_BITS = 0;

        internal const byte MQTT_MSG_PINGREQ_FLAG_BITS = 0;

        internal const byte MQTT_MSG_PINGRESP_FLAG_BITS = 0;

        internal const byte MQTT_MSG_DISCONNECT_FLAG_BITS = 0;

        public const byte QOS_LEVEL_AT_MOST_ONCE = 0;

        public const byte QOS_LEVEL_AT_LEAST_ONCE = 1;

        public const byte QOS_LEVEL_EXACTLY_ONCE = 2;

        public const byte QOS_LEVEL_GRANTED_FAILURE = 128;

        internal const ushort MAX_TOPIC_LENGTH = ushort.MaxValue;

        internal const ushort MIN_TOPIC_LENGTH = 1;

        internal const byte MESSAGE_ID_SIZE = 2;

        protected byte type;

        protected bool dupFlag;

        protected byte qosLevel;

        protected bool retain;

        protected ushort messageId;

        public byte Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
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
            set
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
            set
            {
                retain = value;
            }
        }

        public ushort MessageId
        {
            get
            {
                return messageId;
            }
            set
            {
                messageId = value;
            }
        }

        public abstract byte[] GetBytes(byte protocolVersion);

        protected int encodeRemainingLength(int remainingLength, byte[] buffer, int index)
        {
            int num = 0;
            do
            {
                num = remainingLength % 128;
                remainingLength /= 128;
                if (remainingLength > 0)
                {
                    num |= 0x80;
                }

                buffer[index++] = (byte)num;
            }
            while (remainingLength > 0);
            return index;
        }

        protected static int decodeRemainingLength(IMQTTNetworkChannel channel)
        {
            int num = 1;
            int num2 = 0;
            int num3 = 0;
            byte[] array = new byte[1];
            do
            {
                channel.Receive(array);
                num3 = array[0];
                num2 += (num3 & 0x7F) * num;
                num *= 128;
            }
            while (((uint)num3 & 0x80u) != 0);
            return num2;
        }

        protected string GetTraceString(string name, object[] fieldNames, object[] fieldValues)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(name);
            if (fieldNames != null && fieldValues != null)
            {
                stringBuilder.Append("(");
                bool flag = false;
                for (int i = 0; i < fieldValues.Length; i++)
                {
                    if (fieldValues[i] != null)
                    {
                        if (flag)
                        {
                            stringBuilder.Append(",");
                        }

                        stringBuilder.Append(fieldNames[i]);
                        stringBuilder.Append(":");
                        stringBuilder.Append(GetStringObject(fieldValues[i]));
                        flag = true;
                    }
                }

                stringBuilder.Append(")");
            }

            return stringBuilder.ToString();
        }

        private object GetStringObject(object value)
        {
            if (value is byte[] array)
            {
                string text = "0123456789ABCDEF";
                StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
                for (int i = 0; i < array.Length; i++)
                {
                    stringBuilder.Append(text[array[i] >> 4]);
                    stringBuilder.Append(text[array[i] & 0xF]);
                }

                return stringBuilder.ToString();
            }

            if (value is object[] array2)
            {
                StringBuilder stringBuilder2 = new StringBuilder();
                stringBuilder2.Append('[');
                for (int j = 0; j < array2.Length; j++)
                {
                    if (j > 0)
                    {
                        stringBuilder2.Append(',');
                    }

                    stringBuilder2.Append(array2[j]);
                }

                stringBuilder2.Append(']');
                return stringBuilder2.ToString();
            }

            return value;
        }
    }
}
