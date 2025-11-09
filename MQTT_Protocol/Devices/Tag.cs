using MQTT_Protocol.DataTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Devices
{
    public class Tag : INotifyPropertyChanged
    {
        private int _TagId;

        private string _TagName;

        private dynamic _Value;

        private string _Topic;

        private byte _QoS;

        private bool _Retain;

        private string _Description;

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
            if (ValueChanged != null) ValueChanged(this.Value);
        }

        /// <summary>
        /// Hàm khởi tạo.
        /// </summary>
        public Tag()
        {

        }

        /// <summary>
        /// Mã Tag.
        /// </summary>
        public int TagId
        {
            get { return _TagId; }
            set { _TagId = value; }
        }

        /// <summary>
        /// Tên Tag.
        /// </summary>
        public string TagName
        {
            get { return _TagName; }
            set { _TagName = value; }
        }

        public string Topic
        {
            get { return _Topic; }
            set { _Topic = value; }
        }

        /// <summary>
        /// Quality of Service
        /// </summary>
        public byte QoS
        {
            get { return _QoS; }
            set { _QoS = value; }
        }

        /// <summary>
        /// Retain
        /// </summary>
        public bool Retain
        {
            get { return _Retain; }
            set { _Retain = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        /// <summary>
        /// Giá trị của Tag.
        /// </summary>
        public dynamic Value
        {
            get { return _Value; }
            set
            {
                if ((_Value != null && _Value.ToString() != value.ToString()) || _Value == null)
                {
                    eventValueChanged?.Invoke(value);
                    eventDataUpdated?.Invoke(value);
                    _Value = value;
                    OnPropertyChanged("Value");
                }
            }
        }
    }
}
