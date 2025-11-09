using MQTT_Protocol.Exceptions;
using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgPingResp : MQTTMsgBase
    {
        public MQTTMsgPingResp()
        {
            type = 13;
        }

        public static MQTTMsgPingResp Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMQTTNetworkChannel channel)
        {
            MQTTMsgPingResp result = new MQTTMsgPingResp();
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
                array[num++] = 208;
            }
            else
            {
                array[num++] = 208;
            }

            array[num++] = 0;
            return array;
        }

        public override string ToString()
        {
            return GetTraceString("PINGRESP", null, null);
        }
    }
}
