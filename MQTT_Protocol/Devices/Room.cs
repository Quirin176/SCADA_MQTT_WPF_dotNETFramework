using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Devices
{
    public class Room
    {
        private int _RoomId;

        private string _RoomName;

        private string _Description;

        private List<Device> _Devices = null;

        public Events.EventConnectionStateChanged eventConnectionStateChanged = null;

        public Room()
        {
            _Devices = new List<Device>();
        }

        public List<Device> Devices
        {
            get { return _Devices; }
            set { _Devices = value; }
        }

        public int RoomId
        {
            get { return _RoomId; }
            set { _RoomId = value; }
        }

        public string RoomName
        {
            get { return _RoomName; }
            set { _RoomName = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
    }
}
