using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol
{
    public static class IPAddressUtility
    {
        public static AddressFamily GetAddressFamily(this IPAddress ipAddress)
        {
            return ipAddress.AddressFamily;
        }
    }
}
