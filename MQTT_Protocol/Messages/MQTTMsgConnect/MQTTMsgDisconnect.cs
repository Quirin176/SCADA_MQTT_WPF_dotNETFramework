using MQTT_Protocol.Exceptions;
using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgDisconnect : MQTTMsgBase
    {
        public MQTTMsgDisconnect()
        {
            type = 14;
        }

        public static MQTTMsgDisconnect Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMQTTNetworkChannel channel)
        {
            MQTTMsgDisconnect result = new MQTTMsgDisconnect();
            if (protocolVersion == 4 && (fixedHeaderFirstByte & 0xFu) != 0)
            {
                throw new MQTTClientException(MQTTClientErrorCode.InvalidFlagBits);
            }

            MQTTMsgBase.decodeRemainingLength(channel);
            return result;
        }

        public override byte[] GetBytes(byte protocolVersion)
        {
            byte[] array = new byte[2];
            int num = 0;
            if (protocolVersion == 4)
            {
                array[num++] = 224;
            }
            else
            {
                array[num++] = 224;
            }

            array[num++] = 0;
            return array;
        }

        public override string ToString()
        {
            return GetTraceString("DISCONNECT", null, null);
        }
    }
}
