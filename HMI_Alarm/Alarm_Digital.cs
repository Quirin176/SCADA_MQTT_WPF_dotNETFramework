using MQTT_Protocol.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMI_Alarm
{
    public class Alarm_Digital : INotifyPropertyChanged
    {
        private int _AlarmId;
        private string _AlarmName;
        private string _Source;
        private string _State;

        public Events.EventValueChanged eventValueChanged = null;
        public Events.EventDataUpdated eventDataUpdated = null;

        public delegate void EventValueChanged(dynamic value);
        public event PropertyChangedEventHandler PropertyChanged;
        public EventValueChanged ValueChanged = null;

        protected virtual void OnPropertyChanged(string newName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(newName));
            }
            if (ValueChanged != null) ValueChanged(this.State);
        }

        public int AlarmId
        {
            get { return _AlarmId; }
            set { _AlarmId = value; }
        }

        public string State
        {
            get { return _State; }
            set
            {
                if ((_State != null && _State.ToString() != value.ToString()) || _State == null)
                {
                    eventValueChanged?.Invoke(value);
                    eventDataUpdated?.Invoke(value);
                    _State = value;
                    OnPropertyChanged("State");
                }
            }
        }

        public Alarm_Digital()
        {

        }

        public string AlarmName
        {
            get { return _AlarmName; }
            set { _AlarmName = value; }
        }

        public string Source
        {
            get { return _Source; }
            set { _Source = value; }
        }
    }
}
