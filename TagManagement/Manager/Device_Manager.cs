using MQTT_Protocol.DataTypes;
using MQTT_Protocol.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Driver_Tool.Manager
{
    public class Device_Manager
    {
        public const string DEVICE = "Device";
        public const string DEVICE_ID = "DeviceId";
        public const string DEVICE_NAME = "DeviceName";

        public static void Add(Room room, Device device)
        {
            try
            {
                if (device == null) throw new NullReferenceException("The Device is null reference exception");
                Device fDv = IsExisted(room, device);
                if (fDv != null) throw new Exception(string.Format("Device name: '{0}' is existed", device.DeviceName));
                room.Devices.Add(device);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Update(Room room, Device device)
        {
            try
            {
                if (device == null) throw new NullReferenceException("The Device is null reference exception");
                Device fCh = IsExisted(room, device);
                if (fCh != null) throw new Exception(string.Format("Device name: '{0}' is existed", device.DeviceName));
                foreach (Device item in room.Devices)
                {
                    if (item.DeviceId == device.DeviceId)
                    {
                        item.DeviceId = device.DeviceId;
                        item.DeviceName = device.DeviceName;
                        item.Description = device.Description;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Room room, int grId)
        {
            try
            {
                Device result = GetByDeviceId(room, grId);
                if (result == null) throw new KeyNotFoundException("Device Id is not found exception");
                room.Devices.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Room room, string grName)
        {
            try
            {
                Device result = GetByDeviceName(room, grName);
                if (result == null) throw new KeyNotFoundException("Device name is not found exception");
                room.Devices.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Room room, Device gr)
        {
            try
            {
                if (gr == null) throw new NullReferenceException("The Device is null reference exception");
                foreach (Device item in room.Devices)
                {
                    if (item.DeviceId == gr.DeviceId)
                    {
                        room.Devices.Remove(item);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Device IsExisted(Room room, Device device)
        {
            Device result = null;
            try
            {
                foreach (Device item in room.Devices)
                {
                    if (item.DeviceId != device.DeviceId && item.DeviceName.Equals(device.DeviceName))
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

        public static Device GetByDeviceId(Room ch, int chId)
        {
            Device result = null;
            try
            {
                foreach (Device item in ch.Devices)
                {
                    if (item.DeviceId == chId)
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

        public static Device GetByDeviceName(Room ch, string chName)
        {
            Device result = null;
            try
            {
                foreach (Device item in ch.Devices)
                {
                    if (item.DeviceName.Equals(chName))
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

        public static List<Device> GetDevices(XmlNode dvNode)
        {
            List<Device> deviceList = new List<Device>();
            try
            {
                foreach (XmlNode deviceNote in dvNode)
                {
                    Device device = new Device();
                    device.DeviceId = int.Parse(deviceNote.Attributes[DEVICE_ID].Value);
                    device.DeviceName = deviceNote.Attributes[DEVICE_NAME].Value;
                    device.Description = deviceNote.Attributes[Building_Manager.DESCRIPTION].Value;
                    device.Tags = Tag_Manager.GetTags(deviceNote);
                    deviceList.Add(device);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return deviceList;
        }
    }
}
