using MQTT_Protocol.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Driver_Tool.Manager
{
    public class Room_Manager
    {
        public const string ROOM = "Room";
        public const string ROOM_ID = "RoomId";
        public const string ROOM_NAME = "RoomName";

        public static void Add(Floor ch, Room dv)
        {
            try
            {
                if (dv == null) throw new NullReferenceException("The Room is null reference exception");
                Room fDv = IsExisted(ch, dv);
                if (fDv != null) throw new Exception(string.Format("Room name: '{0}' is existed", dv.RoomName));
                ch.Rooms.Add(dv);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Update(Floor ch, Room dv)
        {
            try
            {
                if (dv == null) throw new NullReferenceException("The Room is null reference exception");
                Room fCh = IsExisted(ch, dv);
                if (fCh != null) throw new Exception(string.Format("Room name: '{0}' is existed", dv.RoomName));
                foreach (Room item in ch.Rooms)
                {
                    if (item.RoomId == dv.RoomId)
                    {
                        item.RoomId = dv.RoomId;
                        item.RoomName = dv.RoomName;
                        item.Description = dv.Description;
                        item.Devices = dv.Devices;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Floor ch, int chId)
        {
            try
            {
                Room result = GetByRoomId(ch, chId);
                if (result == null) throw new KeyNotFoundException("Room Id is not found exception");
                ch.Rooms.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Floor ch, string chName)
        {
            try
            {
                Room result = GetByRoomName(ch, chName);
                if (result == null) throw new KeyNotFoundException("Room name is not found exception");
                ch.Rooms.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Floor ch, Room dv)
        {
            try
            {
                if (dv == null) throw new NullReferenceException("The Room is null reference exception");
                foreach (Room item in ch.Rooms)
                {
                    if (item.RoomId == dv.RoomId)
                    {
                        ch.Rooms.Remove(item);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Room IsExisted(Floor ch, Room dv)
        {
            Room result = null;
            try
            {
                foreach (Room item in ch.Rooms)
                {
                    if (item.RoomId != dv.RoomId && item.RoomName.Equals(dv.RoomName))
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

        public static Room GetByRoomId(Floor ch, int chId)
        {
            Room result = null;
            try
            {
                foreach (Room item in ch.Rooms)
                {
                    if (item.RoomId == chId)
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

        public static Room GetByRoomName(Floor ch, string chName)
        {
            Room result = null;
            try
            {
                foreach (Room item in ch.Rooms)
                {
                    if (item.RoomName.Equals(chName))
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

        public static List<Room> GetRooms(XmlNode chNode)
        {
            List<Room> dvList = new List<Room>();
            try
            {
                foreach (XmlNode dvNode in chNode)
                {
                    Room newDevice = new Room();
                    newDevice.RoomId = int.Parse(dvNode.Attributes[ROOM_ID].Value);
                    newDevice.RoomName = dvNode.Attributes[ROOM_NAME].Value;
                    newDevice.Description = dvNode.Attributes[Building_Manager.DESCRIPTION].Value;
                    newDevice.Devices = Device_Manager.GetDevices(dvNode);
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
