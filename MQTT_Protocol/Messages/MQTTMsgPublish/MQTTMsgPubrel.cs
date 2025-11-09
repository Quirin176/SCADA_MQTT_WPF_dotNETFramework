using MQTT_Protocol.Exceptions;
using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgPubrel : MQTTMsgBase
    {
        public MQTTMsgPubrel()
        {
            type = 6;
            qosLevel = 1;
        }

        public override byte[] GetBytes(byte protocolVersion)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            num2 += 2;
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
            if (protocolVersion == 4)
            {
                array[num5++] = 98;
            }
            else
            {
                array[num5] = (byte)(0x60u | (uint)(qosLevel << 1));
                array[num5] |= (byte)(dupFlag ? 8 : 0);
                num5++;
            }

            num5 = encodeRemainingLength(num4, array, num5);
            array[num5++] = (byte)((uint)(messageId >> 8) & 0xFFu);
            array[num5++] = (byte)(messageId & 0xFFu);
            return array;
        }

        public static MQTTMsgPubrel Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMQTTNetworkChannel channel)
        {
            int num = 0;
            MQTTMsgPubrel mqttMsgPubrel = new MQTTMsgPubrel();
            if (protocolVersion == 4 && (fixedHeaderFirstByte & 0xF) != 2)
            {
                throw new MQTTClientException(MQTTClientErrorCode.InvalidFlagBits);
            }

            int num2 = MQTTMsgBase.decodeRemainingLength(channel);
            byte[] array = new byte[num2];
            channel.Receive(array);
            if (protocolVersion == 3)
            {
                mqttMsgPubrel.qosLevel = (byte)((fixedHeaderFirstByte & 6) >> 1);
                mqttMsgPubrel.dupFlag = (fixedHeaderFirstByte & 8) >> 3 == 1;
            }

            mqttMsgPubrel.messageId = (ushort)((uint)(array[num++] << 8) & 0xFF00u);
            mqttMsgPubrel.messageId |= array[num++];
            return mqttMsgPubrel;
        }

        public override string ToString()
        {
            return GetTraceString("PUBREL", new object[1] { "messageId" }, new object[1] { messageId });
        }
    }
}
