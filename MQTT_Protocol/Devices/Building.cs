using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Devices
{
    public class Building
    {
        private int _BuildingId;

        private string _BuildingName;

        private string _Description;

        private List<Floor> _Floors;

        public Building()
        {
            _Floors = new List<Floor>();
        }

        public Building(int buildingId, string buildingName, string desp = null)
            : this()
        {
            this.BuildingId = buildingId;
            this.BuildingName = buildingName;
            this.Description = desp;
        }

        public int BuildingId
        {
            get { return _BuildingId; }
            set { _BuildingId = value; }
        }

        public string BuildingName
        {
            get { return _BuildingName; }
            set { _BuildingName = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        public List<Floor> Floors
        {
            get { return _Floors; }
            set { _Floors = value; }
        }

        public Floor this[string floorName]
        {
            get
            {
                foreach (Floor item in Floors)
                {
                    if (floorName.Equals(item.FloorName)) return item;
                }
                return null;
            }
        }
    }
}
