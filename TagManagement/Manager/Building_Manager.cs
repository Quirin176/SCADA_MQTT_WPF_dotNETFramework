using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using MQTT_Protocol.Devices;

namespace Driver_Tool.Manager
{
    public class Building_Manager
    {
        public const string ROOT = "Root";
        public const string BUILDING = "Building";
        public const string BUILDING_ID = "BuildingId";
        public const string BUILDING_NAME = "BuildingName";
        public const string DESCRIPTION = "Description";

        public const string FLOOR = "Floor";

        public const string XML_NAME_DEFAULT = "TagCollection";

        private static List<Building> _Buildings = new List<Building>();
        public static List<Building> Buildings
        {
            get { return _Buildings; }
            set { _Buildings = value; }
        }

        public static string XmlPath { set; get; }

        public static void Add(Building ch)
        {
            try
            {
                if (ch == null) throw new NullReferenceException("The Building is null reference exception");
                Building fCh = IsExisted(ch);
                if (fCh != null) throw new Exception(string.Format("Building name: '{0}' is existed", ch.BuildingName));
                _Buildings.Add(ch);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Update(Building ch)
        {
            try
            {
                if (ch == null) throw new NullReferenceException("The Building is null reference exception");
                Building fCh = IsExisted(ch);
                if (fCh != null) throw new Exception(string.Format("Building name: '{0}' is existed", ch.BuildingName));
                foreach (Building item in _Buildings)
                {
                    if (item.BuildingId == ch.BuildingId)
                    {
                        item.BuildingName = ch.BuildingName;
                        item.Description = ch.Description;
                        item.Floors = ch.Floors;
                        DIEthernet en = (DIEthernet)item;
                        DIEthernet enParam = (DIEthernet)ch;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(int chId)
        {
            try
            {
                Building result = GetByBuildingId(chId);
                if (result == null) throw new KeyNotFoundException("Building Id is not found exception");
                _Buildings.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(string chName)
        {
            try
            {
                Building result = GetByBuildingName(chName);
                if (result == null) throw new KeyNotFoundException("Building name is not found exception");
                _Buildings.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Building ch)
        {
            try
            {
                if (ch == null) throw new NullReferenceException("The Building is null reference exception");
                foreach (Building item in _Buildings)
                {
                    if (item.BuildingId == ch.BuildingId)
                    {
                        _Buildings.Remove(item);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Building IsExisted(Building ch)
        {
            Building result = null;
            try
            {
                foreach (Building item in _Buildings)
                {
                    if (item.BuildingId != ch.BuildingId && item.BuildingName.Equals(ch.BuildingName))
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

        public static Building GetByBuildingId(int chId)
        {
            Building result = null;
            try
            {
                foreach (Building item in _Buildings)
                {
                    if (item.BuildingId == chId)
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

        public static Building GetByBuildingName(string chName)
        {
            Building result = null;
            try
            {
                foreach (Building item in _Buildings)
                {
                    if (item.BuildingName.Equals(chName))
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

        public static List<Building> GetBuildings()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                if (string.IsNullOrEmpty(XmlPath) || string.IsNullOrWhiteSpace(XmlPath))
                    XmlPath = ReadKey(XML_NAME_DEFAULT);
                xmlDoc.Load(XmlPath);
                var nodes = xmlDoc.SelectNodes(ROOT);
                foreach (XmlNode rootNode in nodes)
                {
                    XmlNodeList channelNodeList = rootNode.SelectNodes(BUILDING);
                    foreach (XmlNode chNode in channelNodeList)
                    {
                        Building newChannel = null;
                        newChannel = new DIEthernet();
                        DIEthernet die = (DIEthernet)newChannel;

                        if (newChannel != null)
                        {
                            newChannel.BuildingId = int.Parse(chNode.Attributes[BUILDING_ID].Value);
                            newChannel.BuildingName = chNode.Attributes[BUILDING_NAME].Value;
                            newChannel.Description = chNode.Attributes[DESCRIPTION].Value;
                            newChannel.Floors = Floor_Manager.GetFloors(chNode);
                            _Buildings.Add(newChannel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return _Buildings;
        }

        public static void CreatFile(string pathXml)
        {
            try
            {
                if (File.Exists(pathXml))
                {
                    File.Delete(pathXml);
                }
                XElement element = new XElement(ROOT);
                XDocument doc = new XDocument(element);
                doc.Save(pathXml);
                XmlPath = pathXml;
                WriteKey(XML_NAME_DEFAULT, pathXml);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string ReadKey(string keyName)
        {
            string result = string.Empty;
            try
            {
                Microsoft.Win32.RegistryKey regKey;
                regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\IndustrialHMI");
                if (regKey != null) result = (string)regKey.GetValue(keyName);
            }
            catch (Exception ex) { throw ex; }
            return result;
        }

        public static void WriteKey(string keyName, string keyValue)
        {
            try
            {
                Microsoft.Win32.RegistryKey regKey;
                regKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\IndustrialHMI");
                regKey.SetValue(keyName, keyValue);
                regKey.Close();
            }
            catch (Exception ex) { throw ex; }
        }


        public static void Save(string pathXml)
        {
            try
            {
                WriteKey(XML_NAME_DEFAULT, pathXml);
                CreatFile(pathXml);
                XmlPath = pathXml;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(pathXml);
                XmlNode root = xmlDoc.SelectSingleNode(ROOT);

                // List Buildings.
                foreach (Building building in Buildings)
                {
                    XmlElement buildingElement = xmlDoc.CreateElement(BUILDING);
                    buildingElement.SetAttribute(BUILDING_ID, string.Format("{0}", building.BuildingId));
                    buildingElement.SetAttribute(BUILDING_NAME, building.BuildingName);
                    buildingElement.SetAttribute(DESCRIPTION, building.Description);
                    root.AppendChild(buildingElement);

                    DIEthernet die = (DIEthernet)building;

                    if (building.Floors.Count == 0) continue;

                    // List Floors.
                    foreach (Floor floor in building.Floors)
                    {
                        XmlElement floorElement = xmlDoc.CreateElement(Floor_Manager.FLOOR);
                        floorElement.SetAttribute(Floor_Manager.FLOOR_ID, string.Format("{0}", floor.FloorId));
                        floorElement.SetAttribute(Floor_Manager.FLOOR_NAME, floor.FloorName);
                        floorElement.SetAttribute(DESCRIPTION, floor.Description);
                        buildingElement.AppendChild(floorElement);
                        if (floor.Rooms.Count == 0) continue;

                        // List Rooms.
                        foreach (Room room in floor.Rooms)
                        {
                            XmlElement roomElement = xmlDoc.CreateElement(Room_Manager.ROOM);
                            roomElement.SetAttribute(Room_Manager.ROOM_ID, string.Format("{0}", room.RoomId));
                            roomElement.SetAttribute(Room_Manager.ROOM_NAME, room.RoomName);
                            roomElement.SetAttribute(DESCRIPTION, room.Description);
                            floorElement.AppendChild(roomElement);
                            if (floor.Rooms.Count == 0) continue;

                            // List Devices.
                            foreach (Device device in room.Devices)
                            {
                                XmlElement deviceElement = xmlDoc.CreateElement(Device_Manager.DEVICE);
                                deviceElement.SetAttribute(Device_Manager.DEVICE_ID, string.Format("{0}", device.DeviceId));
                                deviceElement.SetAttribute(Device_Manager.DEVICE_NAME, device.DeviceName);
                                deviceElement.SetAttribute(DESCRIPTION, device.Description);
                                roomElement.AppendChild(deviceElement);

                                // List Tags.
                                foreach (Tag tg in device.Tags)
                                {
                                    XmlElement tgElement = xmlDoc.CreateElement(Tag_Manager.TAG);
                                    tgElement.SetAttribute(Tag_Manager.TAG_ID, string.Format("{0}", tg.TagId));
                                    tgElement.SetAttribute(Tag_Manager.TAG_NAME, tg.TagName);
                                    tgElement.SetAttribute(Tag_Manager.TOPIC, string.Format("{0}", tg.Topic));
                                    tgElement.SetAttribute(Tag_Manager.QOS, string.Format("{0}", tg.QoS));
                                    tgElement.SetAttribute(Tag_Manager.RETAIN, string.Format("{0}", tg.Retain));
                                    tgElement.SetAttribute(DESCRIPTION, tg.Description);

                                    deviceElement.AppendChild(tgElement);
                                }
                            }
                        }
                    }
                }
                xmlDoc.Save(pathXml);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
