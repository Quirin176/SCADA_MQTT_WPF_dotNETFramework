using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol
{
    public interface IMQTTNetworkChannel
    {
        bool DataAvailable { get; }

        int Receive(byte[] buffer);

        int Receive(byte[] buffer, int timeout);

        int Send(byte[] buffer);

        void Close();

        void Connect();

        void Accept();

    }
}
