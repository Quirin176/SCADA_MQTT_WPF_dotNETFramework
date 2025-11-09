using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol
{
    public static class MQTT_SslUtility
    {
        public static SslProtocols ToSslPlatformEnum(MQTT_SslProtocols mqttSslProtocol)
        {
            switch (mqttSslProtocol)
            {
                case MQTT_SslProtocols.None:
                    return SslProtocols.None;
                case MQTT_SslProtocols.SSLv3:
                    return SslProtocols.Ssl3;
                case MQTT_SslProtocols.TLSv1_0:
                    return SslProtocols.Tls;
                case MQTT_SslProtocols.TLSv1_1:
                    return SslProtocols.Tls11;
                case MQTT_SslProtocols.TLSv1_2:
                    return SslProtocols.Tls12;
                default:
                    throw new ArgumentException("SSL/TLS protocol version not supported");
            }
        }
    }
}
