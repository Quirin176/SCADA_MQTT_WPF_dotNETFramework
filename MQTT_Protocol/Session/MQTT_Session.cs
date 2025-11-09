using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Session
{
    public abstract class MQTT_Session
    {
        public string ClientId { get; set; }

        public Hashtable InflightMessages { get; set; }

        public MQTT_Session()
            : this(null)
        {
        }

        public MQTT_Session(string clientId)
        {
            ClientId = clientId;
            InflightMessages = new Hashtable();
        }

        public virtual void Clear()
        {
            ClientId = null;
            InflightMessages.Clear();
        }
    }
}
