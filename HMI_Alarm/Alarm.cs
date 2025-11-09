using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMI_Alarm
{
    public class Alarm
    {
        public int ID { get; set; }
        public string AlarmName { get; set; }
        public string Source { get; set; }
        public string State { get; set; }
        public DateTime DateTime { get; set; }
        public string Acknowledged { get; set; }
        public string AcknowledgedBy { get; set; }
    }
}
