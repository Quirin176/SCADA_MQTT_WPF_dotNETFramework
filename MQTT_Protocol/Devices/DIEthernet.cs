using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Devices
{
    public sealed class DIEthernet : Building
    {
        private string _IPAddress = "192.168.0.10";

        private short _Port = 1883;

        public DIEthernet()
        {

        }

        public DIEthernet(int buildingId, string buildingName, string ip, short port = 102, string desc = null)
            : this()
        {
            this.BuildingId = buildingId;
            this.BuildingName = buildingName;
            this.IPAddress = ip;
            this.Port = port;
            this.Description = desc;
        }

        public string IPAddress
        {
            get { return _IPAddress; }
            set { _IPAddress = value; }
        }

        public short Port
        {
            get { return _Port; }
            set { _Port = value; }
        }
    }
}
