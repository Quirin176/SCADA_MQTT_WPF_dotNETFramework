using MQTT_Protocol.Exceptions;
using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgConnack : MQTTMsgBase
    {
        public const byte CONN_ACCEPTED = 0;

        public const byte CONN_REFUSED_PROT_VERS = 1;

        public const byte CONN_REFUSED_IDENT_REJECTED = 2;

        public const byte CONN_REFUSED_SERVER_UNAVAILABLE = 3;

        public const byte CONN_REFUSED_USERNAME_PASSWORD = 4;

        public const byte CONN_REFUSED_NOT_AUTHORIZED = 5;

        private const byte TOPIC_NAME_COMP_RESP_BYTE_OFFSET = 0;

        private const byte TOPIC_NAME_COMP_RESP_BYTE_SIZE = 1;

        private const byte CONN_ACK_FLAGS_BYTE_OFFSET = 0;

        private const byte CONN_ACK_FLAGS_BYTE_SIZE = 1;

        private const byte SESSION_PRESENT_FLAG_MASK = 1;

        private const byte SESSION_PRESENT_FLAG_OFFSET = 0;

        private const byte SESSION_PRESENT_FLAG_SIZE = 1;

        private const byte CONN_RETURN_CODE_BYTE_OFFSET = 1;

        private const byte CONN_RETURN_CODE_BYTE_SIZE = 1;

        private bool sessionPresent;

        private byte returnCode;

        public bool SessionPresent
        {
            get
            {
                return sessionPresent;
            }
            set
            {
                sessionPresent = value;
            }
        }

        public byte ReturnCode
        {
            get
            {
                return returnCode;
            }
            set
            {
                returnCode = value;
            }
        }
        public MQTTMsgConnack()
        {
            type = 2;
        }

        public static MQTTMsgConnack Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMQTTNetworkChannel channel)
        {
            MQTTMsgConnack mqttMsgConnack = new MQTTMsgConnack();
            if (protocolVersion == 4 && (fixedHeaderFirstByte & 0xFu) != 0)
            {
                throw new MQTTClientException(MQTTClientErrorCode.InvalidFlagBits);
            }

            int num = MQTTMsgBase.decodeRemainingLength(channel);
            byte[] array = new byte[num];
            channel.Receive(array);
            if (protocolVersion == 4)
            {
                mqttMsgConnack.sessionPresent = (array[0] & 1) != 0;
            }

            mqttMsgConnack.returnCode = array[1];
            return mqttMsgConnack;
        }

        public override byte[] GetBytes(byte ProtocolVersion)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int index = 0;
            num2 = ((ProtocolVersion != 4) ? (num2 + 2) : (num2 + 2));
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
            if (ProtocolVersion == 4)
            {
                array[index++] = 32;
            }
            else
            {
                array[index++] = 32;
            }

            index = encodeRemainingLength(num4, array, index);
            if (ProtocolVersion == 4)
            {
                array[index++] = (byte)(sessionPresent ? 1 : 0);
            }
            else
            {
                array[index++] = 0;
            }

            array[index++] = returnCode;
            return array;
        }

        public override string ToString()
        {
            return GetTraceString("CONNACK", new object[1] { "returnCode" }, new object[1] { returnCode });
        }
    }
}
