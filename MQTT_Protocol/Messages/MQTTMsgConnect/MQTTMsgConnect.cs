using MQTT_Protocol.Exceptions;
using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public class MQTTMsgConnect : MQTTMsgBase
    {
        internal const string PROTOCOL_NAME_V3_1 = "MQIsdp";

        internal const string PROTOCOL_NAME_V3_1_1 = "MQTT";

        internal const int CLIENT_ID_MAX_LENGTH = 23;

        internal const byte PROTOCOL_NAME_LEN_SIZE = 2;

        internal const byte PROTOCOL_NAME_V3_1_SIZE = 6;

        internal const byte PROTOCOL_NAME_V3_1_1_SIZE = 4;

        internal const byte PROTOCOL_VERSION_SIZE = 1;

        internal const byte CONNECT_FLAGS_SIZE = 1;

        internal const byte KEEP_ALIVE_TIME_SIZE = 2;

        internal const byte PROTOCOL_VERSION_V3_1 = 3;

        internal const byte PROTOCOL_VERSION_V3_1_1 = 4;

        internal const ushort KEEP_ALIVE_PERIOD_DEFAULT = 60;

        internal const ushort MAX_KEEP_ALIVE = ushort.MaxValue;

        internal const byte USERNAME_FLAG_MASK = 128;

        internal const byte USERNAME_FLAG_OFFSET = 7;

        internal const byte USERNAME_FLAG_SIZE = 1;

        internal const byte PASSWORD_FLAG_MASK = 64;

        internal const byte PASSWORD_FLAG_OFFSET = 6;

        internal const byte PASSWORD_FLAG_SIZE = 1;

        internal const byte WILL_RETAIN_FLAG_MASK = 32;

        internal const byte WILL_RETAIN_FLAG_OFFSET = 5;

        internal const byte WILL_RETAIN_FLAG_SIZE = 1;

        internal const byte WILL_QOS_FLAG_MASK = 24;

        internal const byte WILL_QOS_FLAG_OFFSET = 3;

        internal const byte WILL_QOS_FLAG_SIZE = 2;

        internal const byte WILL_FLAG_MASK = 4;

        internal const byte WILL_FLAG_OFFSET = 2;

        internal const byte WILL_FLAG_SIZE = 1;

        internal const byte CLEAN_SESSION_FLAG_MASK = 2;

        internal const byte CLEAN_SESSION_FLAG_OFFSET = 1;

        internal const byte CLEAN_SESSION_FLAG_SIZE = 1;

        internal const byte RESERVED_FLAG_MASK = 1;

        internal const byte RESERVED_FLAG_OFFSET = 0;

        internal const byte RESERVED_FLAG_SIZE = 1;

        private string protocolName;

        private byte protocolVersion;

        private string clientId;

        protected bool willRetain;

        protected byte willQosLevel;

        private bool willFlag;

        private string willTopic;

        private string willMessage;

        private string username;

        private string password;

        private bool cleanSession;

        private ushort keepAlivePeriod;

        public string ProtocolName
        {
            get
            {
                return protocolName;
            }
            set
            {
                protocolName = value;
            }
        }

        public byte ProtocolVersion
        {
            get
            {
                return protocolVersion;
            }
            set
            {
                protocolVersion = value;
            }
        }

        public string ClientId
        {
            get
            {
                return clientId;
            }
            set
            {
                clientId = value;
            }
        }

        public bool WillRetain
        {
            get
            {
                return willRetain;
            }
            set
            {
                willRetain = value;
            }
        }

        public byte WillQosLevel
        {
            get
            {
                return willQosLevel;
            }
            set
            {
                willQosLevel = value;
            }
        }

        public bool WillFlag
        {
            get
            {
                return willFlag;
            }
            set
            {
                willFlag = value;
            }
        }

        public string WillTopic
        {
            get
            {
                return willTopic;
            }
            set
            {
                willTopic = value;
            }
        }

        public string WillMessage
        {
            get
            {
                return willMessage;
            }
            set
            {
                willMessage = value;
            }
        }

        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        public bool CleanSession
        {
            get
            {
                return cleanSession;
            }
            set
            {
                cleanSession = value;
            }
        }

        public ushort KeepAlivePeriod
        {
            get
            {
                return keepAlivePeriod;
            }
            set
            {
                keepAlivePeriod = value;
            }
        }

        public MQTTMsgConnect()
        {
            type = 1;
        }

        public MQTTMsgConnect(string clientId)
            : this(clientId, null, null, willRetain: false, 1, willFlag: false, null, null, cleanSession: true, 60, 4)
        {
        }

        public MQTTMsgConnect(string clientId, string username, string password, bool willRetain, byte willQosLevel, bool willFlag, string willTopic, string willMessage, bool cleanSession, ushort keepAlivePeriod, byte protocolVersion)
        {
            type = 1;
            this.clientId = clientId;
            this.username = username;
            this.password = password;
            this.willRetain = willRetain;
            this.willQosLevel = willQosLevel;
            this.willFlag = willFlag;
            this.willTopic = willTopic;
            this.willMessage = willMessage;
            this.cleanSession = cleanSession;
            this.keepAlivePeriod = keepAlivePeriod;
            this.protocolVersion = protocolVersion;
            protocolName = ((this.protocolVersion == 4) ? "MQTT" : "MQIsdp");
        }

        public static MQTTMsgConnect Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMQTTNetworkChannel channel)
        {
            int num = 0;
            MQTTMsgConnect mqttMsgConnect = new MQTTMsgConnect();
            int num2 = MQTTMsgBase.decodeRemainingLength(channel);
            byte[] array = new byte[num2];
            channel.Receive(array);
            int num3 = (array[num++] << 8) & 0xFF00;
            num3 |= array[num++];
            byte[] array2 = new byte[num3];
            Array.Copy(array, num, array2, 0, num3);
            num += num3;
            mqttMsgConnect.protocolName = new string(Encoding.UTF8.GetChars(array2));
            if (!mqttMsgConnect.protocolName.Equals("MQIsdp") && !mqttMsgConnect.protocolName.Equals("MQTT"))
            {
                throw new MQTTClientException(MQTTClientErrorCode.InvalidProtocolName);
            }

            mqttMsgConnect.protocolVersion = array[num];
            num++;
            if (mqttMsgConnect.protocolVersion == 4 && ((uint)array[num] & (true ? 1u : 0u)) != 0)
            {
                throw new MQTTClientException(MQTTClientErrorCode.InvalidConnectFlags);
            }

            bool flag = (array[num] & 0x80) != 0;
            bool flag2 = (array[num] & 0x40) != 0;
            mqttMsgConnect.willRetain = (array[num] & 0x20) != 0;
            mqttMsgConnect.willQosLevel = (byte)((array[num] & 0x18) >> 3);
            mqttMsgConnect.willFlag = (array[num] & 4) != 0;
            mqttMsgConnect.cleanSession = (array[num] & 2) != 0;
            num++;
            mqttMsgConnect.keepAlivePeriod = (ushort)((uint)(array[num++] << 8) & 0xFF00u);
            mqttMsgConnect.keepAlivePeriod |= array[num++];
            int num4 = (array[num++] << 8) & 0xFF00;
            num4 |= array[num++];
            byte[] array3 = new byte[num4];
            Array.Copy(array, num, array3, 0, num4);
            num += num4;
            mqttMsgConnect.clientId = new string(Encoding.UTF8.GetChars(array3));
            if (mqttMsgConnect.protocolVersion == 4 && num4 == 0 && !mqttMsgConnect.cleanSession)
            {
                throw new MQTTClientException(MQTTClientErrorCode.InvalidClientId);
            }

            if (mqttMsgConnect.willFlag)
            {
                int num5 = (array[num++] << 8) & 0xFF00;
                num5 |= array[num++];
                byte[] array4 = new byte[num5];
                Array.Copy(array, num, array4, 0, num5);
                num += num5;
                mqttMsgConnect.willTopic = new string(Encoding.UTF8.GetChars(array4));
                int num6 = (array[num++] << 8) & 0xFF00;
                num6 |= array[num++];
                byte[] array5 = new byte[num6];
                Array.Copy(array, num, array5, 0, num6);
                num += num6;
                mqttMsgConnect.willMessage = new string(Encoding.UTF8.GetChars(array5));
            }

            if (flag)
            {
                int num7 = (array[num++] << 8) & 0xFF00;
                num7 |= array[num++];
                byte[] array6 = new byte[num7];
                Array.Copy(array, num, array6, 0, num7);
                num += num7;
                mqttMsgConnect.username = new string(Encoding.UTF8.GetChars(array6));
            }

            if (flag2)
            {
                int num8 = (array[num++] << 8) & 0xFF00;
                num8 |= array[num++];
                byte[] array7 = new byte[num8];
                Array.Copy(array, num, array7, 0, num8);
                num += num8;
                mqttMsgConnect.password = new string(Encoding.UTF8.GetChars(array7));
            }

            return mqttMsgConnect;
        }

        public override byte[] GetBytes(byte protocolVersion)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int index = 0;
            byte[] bytes = Encoding.UTF8.GetBytes(clientId);
            byte[] array = ((willFlag && willTopic != null) ? Encoding.UTF8.GetBytes(willTopic) : null);
            byte[] array2 = ((willFlag && willMessage != null) ? Encoding.UTF8.GetBytes(willMessage) : null);
            byte[] array3 = ((username != null && username.Length > 0) ? Encoding.UTF8.GetBytes(username) : null);
            byte[] array4 = ((password != null && password.Length > 0) ? Encoding.UTF8.GetBytes(password) : null);
            if (this.protocolVersion == 4)
            {
                if (willFlag && (willQosLevel >= 3 || array == null || array2 == null || (array != null && array.Length == 0) || (array2 != null && array2.Length == 0)))
                {
                    throw new MQTTClientException(MQTTClientErrorCode.WillWrong);
                }

                if (!willFlag && (willRetain || array != null || array2 != null || (array != null && array.Length != 0) || (array2 != null && array2.Length != 0)))
                {
                    throw new MQTTClientException(MQTTClientErrorCode.WillWrong);
                }
            }

            if (keepAlivePeriod > ushort.MaxValue)
            {
                throw new MQTTClientException(MQTTClientErrorCode.KeepAliveWrong);
            }

            if (willQosLevel < 0 || willQosLevel > 2)
            {
                throw new MQTTClientException(MQTTClientErrorCode.WillWrong);
            }

            num2 = ((this.protocolVersion != 3) ? (num2 + 6) : (num2 + 8));
            num2++;
            num2++;
            num2 += 2;
            num3 += bytes.Length + 2;
            num3 += ((array != null) ? (array.Length + 2) : 0);
            num3 += ((array2 != null) ? (array2.Length + 2) : 0);
            num3 += ((array3 != null) ? (array3.Length + 2) : 0);
            num3 += ((array4 != null) ? (array4.Length + 2) : 0);
            num4 += num2 + num3;
            num = 1;
            int num5 = num4;
            do
            {
                num++;
                num5 /= 128;
            }
            while (num5 > 0);
            byte[] array5 = new byte[num + num2 + num3];
            array5[index++] = 16;
            index = encodeRemainingLength(num4, array5, index);
            array5[index++] = 0;
            if (this.protocolVersion == 3)
            {
                array5[index++] = 6;
                Array.Copy(Encoding.UTF8.GetBytes("MQIsdp"), 0, array5, index, 6);
                index += 6;
                array5[index++] = 3;
            }
            else
            {
                array5[index++] = 4;
                Array.Copy(Encoding.UTF8.GetBytes("MQTT"), 0, array5, index, 4);
                index += 4;
                array5[index++] = 4;
            }

            byte b = 0;
            b = (byte)(b | ((array3 != null) ? 128u : 0u));
            b = (byte)(b | ((array4 != null) ? 64u : 0u));
            b = (byte)(b | (willRetain ? 32u : 0u));
            if (willFlag)
            {
                b |= (byte)(willQosLevel << 3);
            }

            b = (byte)(b | (willFlag ? 4u : 0u));
            b = (byte)(b | (cleanSession ? 2u : 0u));
            array5[index++] = b;
            array5[index++] = (byte)((uint)(keepAlivePeriod >> 8) & 0xFFu);
            array5[index++] = (byte)(keepAlivePeriod & 0xFFu);
            array5[index++] = (byte)((uint)(bytes.Length >> 8) & 0xFFu);
            array5[index++] = (byte)((uint)bytes.Length & 0xFFu);
            Array.Copy(bytes, 0, array5, index, bytes.Length);
            index += bytes.Length;
            if (willFlag && array != null)
            {
                array5[index++] = (byte)((uint)(array.Length >> 8) & 0xFFu);
                array5[index++] = (byte)((uint)array.Length & 0xFFu);
                Array.Copy(array, 0, array5, index, array.Length);
                index += array.Length;
            }

            if (willFlag && array2 != null)
            {
                array5[index++] = (byte)((uint)(array2.Length >> 8) & 0xFFu);
                array5[index++] = (byte)((uint)array2.Length & 0xFFu);
                Array.Copy(array2, 0, array5, index, array2.Length);
                index += array2.Length;
            }

            if (array3 != null)
            {
                array5[index++] = (byte)((uint)(array3.Length >> 8) & 0xFFu);
                array5[index++] = (byte)((uint)array3.Length & 0xFFu);
                Array.Copy(array3, 0, array5, index, array3.Length);
                index += array3.Length;
            }

            if (array4 != null)
            {
                array5[index++] = (byte)((uint)(array4.Length >> 8) & 0xFFu);
                array5[index++] = (byte)((uint)array4.Length & 0xFFu);
                Array.Copy(array4, 0, array5, index, array4.Length);
                index += array4.Length;
            }

            return array5;
        }

        public override string ToString()
        {
            return GetTraceString("CONNECT", new object[12]
            {
            "protocolName", "protocolVersion", "clientId", "willFlag", "willRetain", "willQosLevel", "willTopic", "willMessage", "username", "password",
            "cleanSession", "keepAlivePeriod"
            }, new object[12]
            {
            protocolName, protocolVersion, clientId, willFlag, willRetain, willQosLevel, willTopic, willMessage, username, password,
            cleanSession, keepAlivePeriod
            });
        }
    }
}
