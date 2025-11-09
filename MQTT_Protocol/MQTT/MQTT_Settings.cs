using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol
{
    public class MQTT_Settings
    {
        public const int MQTT_BROKER_DEFAULT_PORT = 1883;

        public const int MQTT_BROKER_DEFAULT_SSL_PORT = 8883;

        public const int MQTT_DEFAULT_TIMEOUT = 30000;

        public const int MQTT_ATTEMPTS_RETRY = 3;

        public const int MQTT_DELAY_RETRY = 10000;

        public const int MQTT_CONNECT_TIMEOUT = 30000;

        public const int MQTT_MAX_INFLIGHT_QUEUE_SIZE = int.MaxValue;

        private static MQTT_Settings instance;

        public int Port { get; internal set; }

        public int SslPort { get; internal set; }

        public int TimeoutOnConnection { get; internal set; }

        public int TimeoutOnReceiving { get; internal set; }

        public int AttemptsOnRetry { get; internal set; }

        public int DelayOnRetry { get; internal set; }

        public int InflightQueueSize { get; set; }

        public static MQTT_Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MQTT_Settings();
                }

                return instance;
            }
        }

        private MQTT_Settings()
        {
            Port = 1883;
            SslPort = 8883;
            TimeoutOnReceiving = 30000;
            AttemptsOnRetry = 3;
            DelayOnRetry = 10000;
            TimeoutOnConnection = 30000;
            InflightQueueSize = int.MaxValue;
        }
    }
}
