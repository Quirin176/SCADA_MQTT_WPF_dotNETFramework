using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMI_Alarm
{
    public class Device_Analog
    {
        private int _DeviceAnalogId;

        private string _DeviceAnalogName;

        private List<Alarm_Analog> _AlarmAnalogs;

        public Device_Analog()
        {
            _AlarmAnalogs = new List<Alarm_Analog>();
        }

        public int DeviceAnalogId
        {
            get { return _DeviceAnalogId; }
            set { _DeviceAnalogId = value; }
        }

        public string DeviceAnalogName
        {
            get { return _DeviceAnalogName; }
            set { _DeviceAnalogName = value; }
        }

        public List<Alarm_Analog> AlarmAnalogs
        {
            get { return _AlarmAnalogs; }
            set { _AlarmAnalogs = value; }
        }

        public Alarm_Analog this[string alarmAnalogName]
        {
            get
            {
                foreach (Alarm_Analog item in AlarmAnalogs)
                {
                    if (alarmAnalogName.Equals(item.AlarmName)) return item;
                }
                return null;
            }
        }
    }
}
