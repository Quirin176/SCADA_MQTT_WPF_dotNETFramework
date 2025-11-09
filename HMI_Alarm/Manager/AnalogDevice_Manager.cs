using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace HMI_Alarm.Manager
{
    public class AnalogDevice_Manager
    {
        public const string ROOT = "Root";
        public const string DEVICE = "Device";
        public const string DEVICE_ID = "DeviceId";
        public const string DEVICE_NAME = "DeviceName";

        public const string ALARM = "Alarm";

        public const string XML_NAME_DEFAULT = "AnalogAlarmCollection";

        private static List<Device_Analog> _DeviceAnalogs = new List<Device_Analog>();
        public static List<Device_Analog> DeviceAnalogs
        {
            get { return _DeviceAnalogs; }
            set { _DeviceAnalogs = value; }
        }

        public static string XmlPath { set; get; }

        public static void Add(Device_Analog ch)
        {
            try
            {
                if (ch == null) throw new NullReferenceException("The Device is null reference exception");
                Device_Analog fCh = IsExisted(ch);
                if (fCh != null) throw new Exception(string.Format("Device name: '{0}' is existed", ch.DeviceAnalogName));
                _DeviceAnalogs.Add(ch);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Update(Device_Analog ch)
        {
            try
            {
                if (ch == null) throw new NullReferenceException("The Device is null reference exception");
                Device_Analog fCh = IsExisted(ch);
                if (fCh != null) throw new Exception(string.Format("Device name: '{0}' is existed", ch.DeviceAnalogName));
                foreach (Device_Analog item in _DeviceAnalogs)
                {
                    if (item.DeviceAnalogId == ch.DeviceAnalogId)
                    {
                        item.DeviceAnalogName = ch.DeviceAnalogName;
                        item.AlarmAnalogs = ch.AlarmAnalogs;
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
                Device_Analog result = GetByDeviceAnalogId(chId);
                if (result == null) throw new KeyNotFoundException("Device Id is not found exception");
                _DeviceAnalogs.Remove(result);
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
                Device_Analog result = GetByDeviceAnalogName(chName);
                if (result == null) throw new KeyNotFoundException("Device name is not found exception");
                _DeviceAnalogs.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Device_Analog ch)
        {
            try
            {
                if (ch == null) throw new NullReferenceException("The Device is null reference exception");
                foreach (Device_Analog item in _DeviceAnalogs)
                {
                    if (item.DeviceAnalogId == ch.DeviceAnalogId)
                    {
                        _DeviceAnalogs.Remove(item);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Device_Analog IsExisted(Device_Analog ch)
        {
            Device_Analog result = null;
            try
            {
                foreach (Device_Analog item in _DeviceAnalogs)
                {
                    if (item.DeviceAnalogId != ch.DeviceAnalogId && item.DeviceAnalogName.Equals(ch.DeviceAnalogName))
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

        public static Device_Analog GetByDeviceAnalogId(int chId)
        {
            Device_Analog result = null;
            try
            {
                foreach (Device_Analog item in _DeviceAnalogs)
                {
                    if (item.DeviceAnalogId == chId)
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

        public static Device_Analog GetByDeviceAnalogName(string chName)
        {
            Device_Analog result = null;
            try
            {
                foreach (Device_Analog item in _DeviceAnalogs)
                {
                    if (item.DeviceAnalogName.Equals(chName))
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

        public static List<Device_Analog> GetDeviceAnalogs()
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
                    XmlNodeList channelNodeList = rootNode.SelectNodes(DEVICE);
                    foreach (XmlNode chNode in channelNodeList)
                    {
                        Device_Analog newChannel = null;
                        newChannel = new Device_Analog();

                        if (newChannel != null)
                        {
                            newChannel.DeviceAnalogId = int.Parse(chNode.Attributes[DEVICE_ID].Value);
                            newChannel.DeviceAnalogName = chNode.Attributes[DEVICE_NAME].Value;
                            newChannel.AlarmAnalogs = AnalogAlarm_Manager.GetAlarms(chNode);
                            _DeviceAnalogs.Add(newChannel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return _DeviceAnalogs;
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

                // List Devices.
                foreach (Device_Analog device in DeviceAnalogs)
                {
                    XmlElement deviceElement = xmlDoc.CreateElement(DEVICE);
                    deviceElement.SetAttribute(DEVICE_ID, string.Format("{0}", device.DeviceAnalogId));
                    deviceElement.SetAttribute(DEVICE_NAME, device.DeviceAnalogName);
                    root.AppendChild(deviceElement);

                    if (device.AlarmAnalogs.Count == 0) continue;

                    // List Alarms.
                    foreach (Alarm_Analog alarm in device.AlarmAnalogs)
                    {
                        XmlElement alarmElement = xmlDoc.CreateElement(AnalogAlarm_Manager.ALARM);
                        alarmElement.SetAttribute(AnalogAlarm_Manager.ALARM_ID, string.Format("{0}", alarm.AlarmId));
                        alarmElement.SetAttribute(AnalogAlarm_Manager.ALARM_NAME, alarm.AlarmName);
                        alarmElement.SetAttribute(AnalogAlarm_Manager.SOURCE, alarm.Source);
                        alarmElement.SetAttribute(AnalogAlarm_Manager.HIGH_HIGH, string.Format("{0}", alarm.HighHigh));
                        alarmElement.SetAttribute(AnalogAlarm_Manager.HIGH, string.Format("{0}", alarm.High));
                        alarmElement.SetAttribute(AnalogAlarm_Manager.LOW, string.Format("{0}", alarm.Low));
                        alarmElement.SetAttribute(AnalogAlarm_Manager.LOW_LOW, string.Format("{0}", alarm.LowLow));
                        deviceElement.AppendChild(alarmElement);
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
