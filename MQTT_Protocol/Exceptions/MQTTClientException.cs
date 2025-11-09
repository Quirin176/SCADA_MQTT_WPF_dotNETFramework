using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Exceptions
{
    public class MQTTClientException : Exception
    {
        private MQTTClientErrorCode errorCode;

        public MQTTClientErrorCode ErrorCode
        {
            get
            {
                return errorCode;
            }
            set
            {
                errorCode = value;
            }
        }

        public MQTTClientException(MQTTClientErrorCode errorCode)
        {
            this.errorCode = errorCode;
        }
    }
}
