using MQTT_Protocol.Exceptions;
using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgPuback : MQTTMsgBase
    {
        public MQTTMsgPuback()
        {
            type = 4;
        }

        public override byte[] GetBytes(byte protocolVersion)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int index = 0;
            num2 += 2;
            num4 += num2 + num3;
            num = 1;
            int num5 = num4;
            do
            {
                num++;
                num5 /= 128;
            }
            while (num5 > 0);
            byte[] array = new byte[num + num2 + num3];
            if (protocolVersion == 4)
            {
                array[index++] = 64;
            }
            else
            {
                array[index++] = 64;
            }

            index = encodeRemainingLength(num4, array, index);
            array[index++] = (byte)((uint)(messageId >> 8) & 0xFFu);
            array[index++] = (byte)(messageId & 0xFFu);
            return array;
        }

        public static MQTTMsgPuback Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMQTTNetworkChannel channel)
        {
            int num = 0;
            MQTTMsgPuback mqttMsgPuback = new MQTTMsgPuback();
            if (protocolVersion == 4 && (fixedHeaderFirstByte & 0xFu) != 0)
            {
                throw new MQTTClientException(MQTTClientErrorCode.InvalidFlagBits);
            }

            int num2 = MQTTMsgBase.decodeRemainingLength(channel);
            byte[] array = new byte[num2];
            channel.Receive(array);
            mqttMsgPuback.messageId = (ushort)((uint)(array[num++] << 8) & 0xFF00u);
            mqttMsgPuback.messageId |= array[num++];
            return mqttMsgPuback;
        }

        public override string ToString()
        {
            return GetTraceString("PUBACK", new object[1] { "messageId" }, new object[1] { messageId });
        }
    }
}
