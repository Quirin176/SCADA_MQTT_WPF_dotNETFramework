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
    public class DigitalAlarm_Manager
    {
        public const string ALARM = "Alarm";
        public const string ALARM_ID = "AlarmId";
        public const string ALARM_NAME = "AlarmName";
        public const string SOURCE = "Source";

        public static void Add(Device_Digital ddv, Alarm_Digital dalm)
        {
            try
            {
                if (dalm == null) throw new NullReferenceException("The Alarm is null reference exception");
                IsExisted(ddv, dalm);
                ddv.AlarmDigitals.Add(dalm);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Update(Device_Digital ddv, Alarm_Digital dalm)
        {
            try
            {
                if (dalm == null) throw new NullReferenceException("The Alarm is null reference exception");
                IsExisted(ddv, dalm);
                foreach (Alarm_Digital item in ddv.AlarmDigitals)
                {
                    if (item.AlarmId == dalm.AlarmId)
                    {
                        item.AlarmName = dalm.AlarmName;
                        item.Source = dalm.Source;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Device_Digital ddv, int dalmId)
        {
            try
            {
                Alarm_Digital result = GetByAlarmId(ddv, dalmId);
                if (result == null) throw new KeyNotFoundException("Alarm Id is not found exception");
                ddv.AlarmDigitals.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Device_Digital ddv, string dalmName)
        {
            try
            {
                Alarm_Digital result = GetByAlarmName(ddv, dalmName);
                if (result == null) throw new KeyNotFoundException("Alarm name is not found exception");
                ddv.AlarmDigitals.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Device_Digital ddv, Alarm_Digital dalm)
        {
            try
            {
                if (dalm == null) throw new NullReferenceException("The Alarm is null reference exception");
                foreach (Alarm_Digital item in ddv.AlarmDigitals)
                {
                    if (item.AlarmId == dalm.AlarmId)
                    {
                        ddv.AlarmDigitals.Remove(item);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Alarm_Digital IsExisted(Device_Digital ddv, Alarm_Digital dalm)
        {
            Alarm_Digital result = null;
            try
            {
                foreach (Alarm_Digital item in ddv.AlarmDigitals)
                {
                    if (item.AlarmId != dalm.AlarmId && item.AlarmName.Equals(dalm.AlarmName))
                    {
                        throw new InvalidOperationException(string.Format("Alarm name: '{0}' is existed", dalm.AlarmName));
                    }
                    if (item.AlarmId != dalm.AlarmId && item.Source.Equals(dalm.Source))
                    {
                        throw new InvalidOperationException(string.Format("Source: '{0}' is existed", dalm.Source));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static Alarm_Digital GetByAlarmId(Device_Digital ddv, int dalmId)
        {
            Alarm_Digital result = null;
            try
            {
                foreach (Alarm_Digital item in ddv.AlarmDigitals)
                {
                    if (item.AlarmId == dalmId)
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

        public static Alarm_Digital GetByAlarmName(Device_Digital ddv, string DalmName)
        {
            Alarm_Digital result = null;
            try
            {
                foreach (Alarm_Digital item in ddv.AlarmDigitals)
                {
                    if (item.AlarmName.Equals(DalmName))
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

        public static Alarm_Digital GetBySource(Device_Digital ddv, string tgAddress)
        {
            Alarm_Digital result = null;
            try
            {
                foreach (Alarm_Digital item in ddv.AlarmDigitals)
                {
                    if (item.Source.Equals(tgAddress))
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

        public static List<Alarm_Digital> GetAlarms(XmlNode almNote)
        {
            List<Alarm_Digital> almList = new List<Alarm_Digital>();
            try
            {
                foreach (XmlNode item in almNote)
                {
                    Alarm_Digital newDAlarm = new Alarm_Digital();
                    newDAlarm.AlarmId = int.Parse(item.Attributes[ALARM_ID].Value);
                    newDAlarm.AlarmName = item.Attributes[ALARM_NAME].Value;
                    newDAlarm.Source = item.Attributes[SOURCE].Value;

                    almList.Add(newDAlarm);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return almList;
        }
    }
}
