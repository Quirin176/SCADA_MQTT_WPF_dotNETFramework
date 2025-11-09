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
    public class DigitalDevice_Manager
    {
        public const string ROOT = "Root";
        public const string DEVICE = "Device";
        public const string DEVICE_ID = "DeviceId";
        public const string DEVICE_NAME = "DeviceName";

        public const string ALARM = "Alarm";

        public const string XML_NAME_DEFAULT = "DigitalAlarmCollection";

        private static List<Device_Digital> _DeviceDigitals = new List<Device_Digital>();

        public static void Add(Device_Digital ch)
        {
            try
            {
                if (ch == null) throw new NullReferenceException("The Device is null reference exception");
                Device_Digital fCh = IsExisted(ch);
                if (fCh != null) throw new Exception(string.Format("Device name: '{0}' is existed", ch.DeviceDigitalName));
                _DeviceDigitals.Add(ch);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Update(Device_Digital ch)
        {
            try
            {
                if (ch == null) throw new NullReferenceException("The Device is null reference exception");
                Device_Digital fCh = IsExisted(ch);
                if (fCh != null) throw new Exception(string.Format("Device name: '{0}' is existed", ch.DeviceDigitalName));
                foreach (Device_Digital item in _DeviceDigitals)
                {
                    if (item.DeviceDigitalId == ch.DeviceDigitalId)
                    {
                        item.DeviceDigitalName = ch.DeviceDigitalName;
                        item.AlarmDigitals = ch.AlarmDigitals;
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
                Device_Digital result = GetByDeviceDigitalId(chId);
                if (result == null) throw new KeyNotFoundException("Device Id is not found exception");
                _DeviceDigitals.Remove(result);
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
                Device_Digital result = GetByDeviceDigitalName(chName);
                if (result == null) throw new KeyNotFoundException("Device name is not found exception");
                _DeviceDigitals.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Device_Digital ch)
        {
            try
            {
                if (ch == null) throw new NullReferenceException("The Device is null reference exception");
                foreach (Device_Digital item in _DeviceDigitals)
                {
                    if (item.DeviceDigitalId == ch.DeviceDigitalId)
                    {
                        _DeviceDigitals.Remove(item);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Device_Digital IsExisted(Device_Digital ch)
        {
            Device_Digital result = null;
            try
            {
                foreach (Device_Digital item in _DeviceDigitals)
                {
                    if (item.DeviceDigitalId != ch.DeviceDigitalId && item.DeviceDigitalName.Equals(ch.DeviceDigitalName))
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

        public static Device_Digital GetByDeviceDigitalId(int chId)
        {
            Device_Digital result = null;
            try
            {
                foreach (Device_Digital item in _DeviceDigitals)
                {
                    if (item.DeviceDigitalId == chId)
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

        public static Device_Digital GetByDeviceDigitalName(string chName)
        {
            Device_Digital result = null;
            try
            {
                foreach (Device_Digital item in _DeviceDigitals)
                {
                    if (item.DeviceDigitalName.Equals(chName))
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

        public static List<Device_Digital> GetDeviceDigitals()
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
                        Device_Digital newChannel = null;
                        newChannel = new Device_Digital();

                        if (newChannel != null)
                        {
                            newChannel.DeviceDigitalId = int.Parse(chNode.Attributes[DEVICE_ID].Value);
                            newChannel.DeviceDigitalName = chNode.Attributes[DEVICE_NAME].Value;
                            newChannel.AlarmDigitals = DigitalAlarm_Manager.GetAlarms(chNode);
                            _DeviceDigitals.Add(newChannel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return _DeviceDigitals;
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
                foreach (Device_Digital device in DeviceDigitals)
                {
                    XmlElement deviceElement = xmlDoc.CreateElement(DEVICE);
                    deviceElement.SetAttribute(DEVICE_ID, string.Format("{0}", device.DeviceDigitalId));
                    deviceElement.SetAttribute(DEVICE_NAME, device.DeviceDigitalName);
                    root.AppendChild(deviceElement);

                    if (device.AlarmDigitals.Count == 0) continue;

                    // List Alarms.
                    foreach (Alarm_Digital alarm in device.AlarmDigitals)
                    {
                        XmlElement alarmElement = xmlDoc.CreateElement(DigitalAlarm_Manager.ALARM);
                        alarmElement.SetAttribute(DigitalAlarm_Manager.ALARM_ID, string.Format("{0}", alarm.AlarmId));
                        alarmElement.SetAttribute(DigitalAlarm_Manager.ALARM_NAME, alarm.AlarmName);
                        alarmElement.SetAttribute(DigitalAlarm_Manager.SOURCE, alarm.Source);
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

        public static List<Device_Digital> DeviceDigitals
        {
            get { return _DeviceDigitals; }
            set { _DeviceDigitals = value; }
        }

        public static string XmlPath { set; get; }
    }
}
