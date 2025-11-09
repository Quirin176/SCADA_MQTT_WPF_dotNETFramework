using MQTT_Protocol.Exceptions;
using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgPublish : MQTTMsgBase
    {
        private string topic;

        private byte[] message;

        public string Topic
        {
            get
            {
                return topic;
            }
            set
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
            set
            {
                message = value;
            }
        }

        public MQTTMsgPublish()
        {
            type = 3;
        }

        public MQTTMsgPublish(string topic, byte[] message)
            : this(topic, message, dupFlag: false, 0, retain: false)
        {
        }

        public MQTTMsgPublish(string topic, byte[] message, bool dupFlag, byte qosLevel, bool retain)
        {
            type = 3;
            this.topic = topic;
            this.message = message;
            base.dupFlag = dupFlag;
            base.qosLevel = qosLevel;
            base.retain = retain;
            messageId = 0;
        }

        public override byte[] GetBytes(byte protocolVersion)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            if (topic.IndexOf('#') != -1 || topic.IndexOf('+') != -1)
            {
                throw new MQTTClientException(MQTTClientErrorCode.TopicWildcard);
            }

            if (topic.Length < 1 || topic.Length > 65535)
            {
                throw new MQTTClientException(MQTTClientErrorCode.TopicLength);
            }
            if (qosLevel > 2)
            {
                throw new MQTTClientException(MQTTClientErrorCode.QosNotAllowed);
            }

            byte[] bytes = Encoding.UTF8.GetBytes(topic);
            num2 += bytes.Length + 2;
            if (qosLevel == 1 || qosLevel == 2)
            {
                num2 += 2;
            }

            if (message != null)
            {
                num3 += message.Length;
            }

            num4 += num2 + num3;
            num = 1;
            int num6 = num4;
            do
            {
                num++;
                num6 /= 128;
            }
            while (num6 > 0);
            byte[] array = new byte[num + num2 + num3];
            array[num5] = (byte)(0x30u | (uint)(qosLevel << 1));
            array[num5] |= (byte)(dupFlag ? 8 : 0);
            array[num5] |= (byte)(retain ? 1 : 0);
            num5++;
            num5 = encodeRemainingLength(num4, array, num5);
            array[num5++] = (byte)((uint)(bytes.Length >> 8) & 0xFFu);
            array[num5++] = (byte)((uint)bytes.Length & 0xFFu);
            Array.Copy(bytes, 0, array, num5, bytes.Length);
            num5 += bytes.Length;
            if (qosLevel == 1 || qosLevel == 2)
            {
                if (messageId == 0)
                {
                    throw new MQTTClientException(MQTTClientErrorCode.WrongMessageId);
                }

                array[num5++] = (byte)((uint)(messageId >> 8) & 0xFFu);
                array[num5++] = (byte)(messageId & 0xFFu);
            }

            if (message != null)
            {
                Array.Copy(message, 0, array, num5, message.Length);
                num5 += message.Length;
            }

            return array;
        }

        public static MQTTMsgPublish Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMQTTNetworkChannel channel)
        {
            int num = 0;
            MQTTMsgPublish mqttMsgPublish = new MQTTMsgPublish();
            int num2 = MQTTMsgBase.decodeRemainingLength(channel);
            byte[] array = new byte[num2];
            int num3 = channel.Receive(array);
            int num4 = (array[num++] << 8) & 0xFF00;
            num4 |= array[num++];
            byte[] array2 = new byte[num4];
            Array.Copy(array, num, array2, 0, num4);
            num += num4;
            mqttMsgPublish.topic = new string(Encoding.UTF8.GetChars(array2));
            mqttMsgPublish.qosLevel = (byte)((fixedHeaderFirstByte & 6) >> 1);
            if (mqttMsgPublish.qosLevel > 2)
            {
                throw new MQTTClientException(MQTTClientErrorCode.QosNotAllowed);
            }

            mqttMsgPublish.dupFlag = (fixedHeaderFirstByte & 8) >> 3 == 1;
            mqttMsgPublish.retain = (fixedHeaderFirstByte & 1) == 1;
            if (mqttMsgPublish.qosLevel == 1 || mqttMsgPublish.qosLevel == 2)
            {
                mqttMsgPublish.messageId = (ushort)((uint)(array[num++] << 8) & 0xFF00u);
                mqttMsgPublish.messageId |= array[num++];
            }

            int num5 = num2 - num;
            int num6 = num5;
            int num7 = 0;
            mqttMsgPublish.message = new byte[num5];
            Array.Copy(array, num, mqttMsgPublish.message, num7, num3 - num);
            num6 -= num3 - num;
            num7 += num3 - num;
            while (num6 > 0)
            {
                num3 = channel.Receive(array);
                Array.Copy(array, 0, mqttMsgPublish.message, num7, num3);
                num6 -= num3;
                num7 += num3;
            }

            return mqttMsgPublish;
        }

        public override string ToString()
        {
            return GetTraceString("PUBLISH", new object[3] { "messageId", "topic", "message" }, new object[3] { messageId, topic, message });
        }
    }
}
