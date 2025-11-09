using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Devices
{
    public class Floor
    {
        private int _FloorId;

        private string _FloorName;

        private string _Description;

        private List<Room> _Rooms = null;

        public Events.EventConnectionStateChanged eventConnectionStateChanged = null;

        public Floor()
        {
            _Rooms = new List<Room>();
        }

        public List<Room> Rooms
        {
            get { return _Rooms; }
            set { _Rooms = value; }
        }

        public int FloorId
        {
            get { return _FloorId; }
            set { _FloorId = value; }
        }

        public string FloorName
        {
            get { return _FloorName; }
            set { _FloorName = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
    }
}
