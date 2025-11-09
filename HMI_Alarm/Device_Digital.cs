using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMI_Alarm
{
    public class Device_Digital
    {
        private int _DeviceDigitalId;

        private string _DeviceDigitalName;

        private List<Alarm_Digital> _AlarmDigitals;

        public Device_Digital()
        {
            _AlarmDigitals = new List<Alarm_Digital>();
        }

        public int DeviceDigitalId
        {
            get { return _DeviceDigitalId; }
            set { _DeviceDigitalId = value; }
        }

        public string DeviceDigitalName
        {
            get { return _DeviceDigitalName; }
            set { _DeviceDigitalName = value; }
        }

        public List<Alarm_Digital> AlarmDigitals
        {
            get { return _AlarmDigitals; }
            set { _AlarmDigitals = value; }
        }

        public Alarm_Digital this[string alarmDigitalName]
        {
            get
            {
                foreach (Alarm_Digital item in AlarmDigitals)
                {
                    if (alarmDigitalName.Equals(item.AlarmName)) return item;
                }
                return null;
            }
        }
    }
}
