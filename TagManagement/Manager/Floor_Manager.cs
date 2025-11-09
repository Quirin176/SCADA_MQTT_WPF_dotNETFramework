using MQTT_Protocol.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Driver_Tool.Manager
{
    public class Floor_Manager
    {
        public const string FLOOR = "Floor";
        public const string FLOOR_ID = "FloorId";
        public const string FLOOR_NAME = "FloorName";

        public static void Add(Building ch, Floor dv)
        {
            try
            {
                if (dv == null) throw new NullReferenceException("The Floor is null reference exception");
                Floor fDv = IsExisted(ch, dv);
                if (fDv != null) throw new Exception(string.Format("Floor name: '{0}' is existed", dv.FloorName));
                ch.Floors.Add(dv);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Update(Building ch, Floor dv)
        {
            try
            {
                if (dv == null) throw new NullReferenceException("The Floor is null reference exception");
                Floor fCh = IsExisted(ch, dv);
                if (fCh != null) throw new Exception(string.Format("Floor name: '{0}' is existed", dv.FloorName));
                foreach (Floor item in ch.Floors)
                {
                    if (item.FloorId == dv.FloorId)
                    {
                        item.FloorId = dv.FloorId;
                        item.FloorName = dv.FloorName;
                        item.Description = dv.Description;
                        item.Rooms = dv.Rooms;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Building ch, int chId)
        {
            try
            {
                Floor result = GetByFloorId(ch, chId);
                if (result == null) throw new KeyNotFoundException("Floor Id is not found exception");
                ch.Floors.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Building ch, string chName)
        {
            try
            {
                Floor result = GetByFloorName(ch, chName);
                if (result == null) throw new KeyNotFoundException("Floor name is not found exception");
                ch.Floors.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Building ch, Floor dv)
        {
            try
            {
                if (dv == null) throw new NullReferenceException("The Floor is null reference exception");
                foreach (Floor item in ch.Floors)
                {
                    if (item.FloorId == dv.FloorId)
                    {
                        ch.Floors.Remove(item);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Floor IsExisted(Building ch, Floor dv)
        {
            Floor result = null;
            try
            {
                foreach (Floor item in ch.Floors)
                {
                    if (item.FloorId != dv.FloorId && item.FloorName.Equals(dv.FloorName))
                    {
                        result = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static Floor GetByFloorId(Building ch, int chId)
        {
            Floor result = null;
            try
            {
                foreach (Floor item in ch.Floors)
                {
                    if (item.FloorId == chId)
                    {
                        result = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static Floor GetByFloorName(Building ch, string chName)
        {
            Floor result = null;
            try
            {
                foreach (Floor item in ch.Floors)
                {
                    if (item.FloorName.Equals(chName))
                    {
                        result = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static List<Floor> GetFloors(XmlNode chNode)
        {
            List<Floor> dvList = new List<Floor>();
            try
            {
                foreach (XmlNode dvNode in chNode)
                {
                    Floor newDevice = new Floor();
                    newDevice.FloorId = int.Parse(dvNode.Attributes[FLOOR_ID].Value);
                    newDevice.FloorName = dvNode.Attributes[FLOOR_NAME].Value;
                    newDevice.Description = dvNode.Attributes[Building_Manager.DESCRIPTION].Value;
                    newDevice.Rooms = Room_Manager.GetRooms(dvNode);
                    dvList.Add(newDevice);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dvList;
        }
    }
}
