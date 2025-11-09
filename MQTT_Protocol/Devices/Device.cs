using MQTT_Protocol.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Devices
{
    public class Device
    {
        private int _DeviceId;

        private string _DeviceName;

        private string _Description;

        private List<Tag> _Tags = null;

        public Device()
        {
            _Tags = new List<Tag>();
        }

        public List<Tag> Tags
        {
            get { return _Tags; }
            set { _Tags = value; }
        }

        public Device(int deviceId, string deviceName, string deviceDescription)
            : this()
        {
            DeviceId = deviceId;
            DeviceName = deviceName;
            Description = deviceDescription;
        }

        public int DeviceId
        {
            get { return _DeviceId; }
            set { _DeviceId = value; }
        }

        public string DeviceName
        {
            get { return _DeviceName; }
            set { _DeviceName = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
    }
}
