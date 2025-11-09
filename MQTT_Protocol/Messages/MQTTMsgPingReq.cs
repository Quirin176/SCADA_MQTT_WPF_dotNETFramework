using MQTT_Protocol.Exceptions;
using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgPingReq : MQTTMsgBase
    {
        public MQTTMsgPingReq()
        {
            type = 12;
        }

        public override byte[] GetBytes(byte protocolVersion)
        {
            byte[] array = new byte[2];
            int num = 0;
            if (protocolVersion == 4)
            {
                array[num++] = 192;
            }
            else
            {
                array[num++] = 192;
            }
            array[num++] = 0;
            return array;
        }

        public static MQTTMsgPingReq Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMQTTNetworkChannel channel)
        {
            MQTTMsgPingReq result = new MQTTMsgPingReq();
            if (protocolVersion == 4 && (fixedHeaderFirstByte & 0xFu) != 0)
            {
                throw new MQTTClientException(MQTTClientErrorCode.InvalidFlagBits);
            }

            MQTTMsgBase.decodeRemainingLength(channel);
            return result;
        }

        public override string ToString()
        {
            return GetTraceString("PINGREQ", null, null);
        }
    }
}
