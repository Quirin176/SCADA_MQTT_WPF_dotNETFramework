using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Exceptions
{
    public class MQTTConnectionException : System.Exception
    {
        public MQTTConnectionException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
