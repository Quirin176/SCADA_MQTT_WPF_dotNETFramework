using MQTT_Protocol.Exceptions;
using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgSuback : MQTTMsgBase
    {
        private byte[] grantedQosLevels;

        public byte[] GrantedQoSLevels
        {
            get
            {
                return grantedQosLevels;
            }
            set
            {
                grantedQosLevels = value;
            }
        }
        public MQTTMsgSuback()
        {
            type = 9;
        }

        public static MQTTMsgSuback Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMQTTNetworkChannel channel)
        {
            int num = 0;
            MQTTMsgSuback mqttMsgSuback = new MQTTMsgSuback();
            if (protocolVersion == 4 && (fixedHeaderFirstByte & 0xFu) != 0)
            {
                throw new MQTTClientException(MQTTClientErrorCode.InvalidFlagBits);
            }

            int num2 = MQTTMsgBase.decodeRemainingLength(channel);
            byte[] array = new byte[num2];
            channel.Receive(array);
            mqttMsgSuback.messageId = (ushort)((uint)(array[num++] << 8) & 0xFF00u);
            mqttMsgSuback.messageId |= array[num++];
            mqttMsgSuback.grantedQosLevels = new byte[num2 - 2];
            int num3 = 0;
            do
            {
                mqttMsgSuback.grantedQosLevels[num3++] = array[num++];
            }
            while (num < num2);
            return mqttMsgSuback;
        }

        public override byte[] GetBytes(byte protocolVersion)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int index = 0;
            num2 += 2;
            int num5 = 0;
            for (num5 = 0; num5 < grantedQosLevels.Length; num5++)
            {
                num3++;
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
            if (protocolVersion == 4)
            {
                array[index++] = 144;
            }
            else
            {
                array[index++] = 144;
            }

            index = encodeRemainingLength(num4, array, index);
            array[index++] = (byte)((uint)(messageId >> 8) & 0xFFu);
            array[index++] = (byte)(messageId & 0xFFu);
            for (num5 = 0; num5 < grantedQosLevels.Length; num5++)
            {
                array[index++] = grantedQosLevels[num5];
            }

            return array;
        }

        public override string ToString()
        {
            return GetTraceString("SUBACK", new object[2] { "messageId", "grantedQosLevels" }, new object[2] { messageId, grantedQosLevels });
        }
    }
}
