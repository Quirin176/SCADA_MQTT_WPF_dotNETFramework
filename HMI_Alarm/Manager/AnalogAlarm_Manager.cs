using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace HMI_Alarm.Manager
{
    public class AnalogAlarm_Manager
    {
        public const string ALARM = "Alarm";
        public const string ALARM_ID = "AlarmId";
        public const string ALARM_NAME = "AlarmName";
        public const string SOURCE = "Source";

        public const string HIGH_HIGH = "HighHigh";
        public const string HIGH = "High";
        public const string LOW = "Low";
        public const string LOW_LOW = "LowLow";

        public static void Add(Device_Analog adv, Alarm_Analog aalm)
        {
            try
            {
                if (aalm == null) throw new NullReferenceException("The Alarm is null reference exception");
                //IsExisted(adv, aalm);
                adv.AlarmAnalogs.Add(aalm);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Update(Device_Analog adv, Alarm_Analog aalm)
        {
            try
            {
                if (aalm == null) throw new NullReferenceException("The Alarm is null reference exception");
                //IsExisted(adv, aalm);
                foreach (Alarm_Analog item in adv.AlarmAnalogs)
                {
                    if (item.AlarmId == aalm.AlarmId)
                    {
                        item.AlarmName = aalm.AlarmName;
                        item.Source = aalm.Source;

                        item.HighHigh = aalm.HighHigh;
                        item.High = aalm.High;
                        item.Low = aalm.Low;
                        item.LowLow = aalm.LowLow;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Device_Analog adv, int aalmId)
        {
            try
            {
                Alarm_Analog result = GetByAlarmId(adv, aalmId);
                if (result == null) throw new KeyNotFoundException("Alarm Id is not found exception");
                adv.AlarmAnalogs.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Device_Analog adv, string aalmName)
        {
            try
            {
                Alarm_Analog result = GetByAlarmName(adv, aalmName);
                if (result == null) throw new KeyNotFoundException("Alarm name is not found exception");
                adv.AlarmAnalogs.Remove(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(Device_Analog adv, Alarm_Analog aalm)
        {
            try
            {
                if (aalm == null) throw new NullReferenceException("The Alarm is null reference exception");
                foreach (Alarm_Analog item in adv.AlarmAnalogs)
                {
                    if (item.AlarmId == aalm.AlarmId)
                    {
                        adv.AlarmAnalogs.Remove(item);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Alarm_Analog IsExisted(Device_Analog adv, Alarm_Analog aalm)
        {
            Alarm_Analog result = null;
            try
            {
                foreach (Alarm_Analog item in adv.AlarmAnalogs)
                {
                    if (item.AlarmId != aalm.AlarmId && item.AlarmName.Equals(aalm.AlarmName))
                    {
                        throw new InvalidOperationException(string.Format("Alarm name: '{0}' is existed", aalm.AlarmName));
                    }
                    if (item.AlarmId != aalm.AlarmId && item.Source.Equals(aalm.Source))
                    {
                        throw new InvalidOperationException(string.Format("Source: '{0}' is existed", aalm.Source));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static Alarm_Analog GetByAlarmId(Device_Analog adv, int aalmId)
        {
            Alarm_Analog result = null;
            try
            {
                foreach (Alarm_Analog item in adv.AlarmAnalogs)
                {
                    if (item.AlarmId == aalmId)
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

        public static Alarm_Analog GetByAlarmName(Device_Analog adv, string AalmName)
        {
            Alarm_Analog result = null;
            try
            {
                foreach (Alarm_Analog item in adv.AlarmAnalogs)
                {
                    if (item.AlarmName.Equals(AalmName))
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

        public static Alarm_Analog GetBySource(Device_Analog ddv, string tgAddress)
        {
            Alarm_Analog result = null;
            try
            {
                foreach (Alarm_Analog item in ddv.AlarmAnalogs)
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

        public static List<Alarm_Analog> GetAlarms(XmlNode almNote)
        {
            List<Alarm_Analog> almList = new List<Alarm_Analog>();
            try
            {
                foreach (XmlNode item in almNote)
                {
                    Alarm_Analog newAAlarm = new Alarm_Analog();
                    newAAlarm.AlarmId = int.Parse(item.Attributes[ALARM_ID].Value);
                    newAAlarm.AlarmName = item.Attributes[ALARM_NAME].Value;
                    newAAlarm.Source = item.Attributes[SOURCE].Value;
                    newAAlarm.HighHigh = int.Parse(item.Attributes[HIGH_HIGH].Value);
                    newAAlarm.High = int.Parse(item.Attributes[HIGH].Value);
                    newAAlarm.Low = int.Parse(item.Attributes[LOW].Value);
                    newAAlarm.LowLow = int.Parse(item.Attributes[LOW_LOW].Value);

                    almList.Add(newAAlarm);
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
