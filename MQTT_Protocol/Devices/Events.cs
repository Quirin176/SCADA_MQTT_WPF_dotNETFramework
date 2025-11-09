using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Devices
{
    public class Events
    {
        public delegate void EventValueChanged(dynamic value);
        public delegate void EventDataUpdated(dynamic value);
        public delegate void EventConnectionStateChanged(bool status);
    }
}
