using MQTT_Protocol.Exceptions;
using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgUnsubscribe : MQTTMsgBase
    {
        private string[] topics;

        public string[] Topics
        {
            get
            {
                return topics;
            }
            set
            {
                topics = value;
            }
        }

        public MQTTMsgUnsubscribe()
        {
            type = 10;
        }

        public MQTTMsgUnsubscribe(string[] topics)
        {
            type = 10;
            this.topics = topics;
            qosLevel = 1;
        }

        public static MQTTMsgUnsubscribe Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMQTTNetworkChannel channel)
        {
            int num = 0;
            MQTTMsgUnsubscribe mqttMsgUnsubscribe = new MQTTMsgUnsubscribe();
            if (protocolVersion == 4 && (fixedHeaderFirstByte & 0xF) != 2)
            {
                throw new MQTTClientException(MQTTClientErrorCode.InvalidFlagBits);
            }

            int num2 = MQTTMsgBase.decodeRemainingLength(channel);
            byte[] array = new byte[num2];
            channel.Receive(array);
            if (protocolVersion == 3)
            {
                mqttMsgUnsubscribe.qosLevel = (byte)((fixedHeaderFirstByte & 6) >> 1);
                mqttMsgUnsubscribe.dupFlag = (fixedHeaderFirstByte & 8) >> 3 == 1;
                mqttMsgUnsubscribe.retain = false;
            }

            mqttMsgUnsubscribe.messageId = (ushort)((uint)(array[num++] << 8) & 0xFF00u);
            mqttMsgUnsubscribe.messageId |= array[num++];
            IList<string> list = new List<string>();
            do
            {
                int num3 = (array[num++] << 8) & 0xFF00;
                num3 |= array[num++];
                byte[] array2 = new byte[num3];
                Array.Copy(array, num, array2, 0, num3);
                num += num3;
                list.Add(new string(Encoding.UTF8.GetChars(array2)));
            }
            while (num < num2);
            mqttMsgUnsubscribe.topics = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                mqttMsgUnsubscribe.topics[i] = list[i];
            }

            return mqttMsgUnsubscribe;
        }

        public override byte[] GetBytes(byte protocolVersion)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            if (topics == null || topics.Length == 0)
            {
                throw new MQTTClientException(MQTTClientErrorCode.TopicsEmpty);
            }

            num2 += 2;
            int num6 = 0;
            byte[][] array = new byte[topics.Length][];
            for (num6 = 0; num6 < topics.Length; num6++)
            {
                if (topics[num6].Length < 1 || topics[num6].Length > 65535)
                {
                    throw new MQTTClientException(MQTTClientErrorCode.TopicLength);
                }

                array[num6] = Encoding.UTF8.GetBytes(topics[num6]);
                num3 += 2;
                num3 += array[num6].Length;
            }

            num4 += num2 + num3;
            num = 1;
            int num7 = num4;
            do
            {
                num++;
                num7 /= 128;
            }
            while (num7 > 0);
            byte[] array2 = new byte[num + num2 + num3];
            if (protocolVersion == 4)
            {
                array2[num5++] = 162;
            }
            else
            {
                array2[num5] = (byte)(0xA0u | (uint)(qosLevel << 1));
                array2[num5] |= (byte)(dupFlag ? 8 : 0);
                num5++;
            }

            num5 = encodeRemainingLength(num4, array2, num5);
            if (messageId == 0)
            {
                throw new MQTTClientException(MQTTClientErrorCode.WrongMessageId);
            }

            array2[num5++] = (byte)((uint)(messageId >> 8) & 0xFFu);
            array2[num5++] = (byte)(messageId & 0xFFu);
            num6 = 0;
            for (num6 = 0; num6 < topics.Length; num6++)
            {
                array2[num5++] = (byte)((uint)(array[num6].Length >> 8) & 0xFFu);
                array2[num5++] = (byte)((uint)array[num6].Length & 0xFFu);
                Array.Copy(array[num6], 0, array2, num5, array[num6].Length);
                num5 += array[num6].Length;
            }

            return array2;
        }

        public override string ToString()
        {
            return GetTraceString("UNSUBSCRIBE", new object[2] { "messageId", "topics" }, new object[2] { messageId, topics });
        }
    }
}
