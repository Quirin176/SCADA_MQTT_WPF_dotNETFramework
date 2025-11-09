using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol.Messages
{
    public enum MQTTMsgState
    {
        QueuedQos0,
        QueuedQos1,
        QueuedQos2,
        WaitForPuback,
        WaitForPubrec,
        WaitForPubrel,
        WaitForPubcomp,
        SendPubrec,
        SendPubrel,
        SendPubcomp,
        SendPuback,
        SendSubscribe,
        SendUnsubscribe,
        WaitForSuback,
        WaitForUnsuback
    }
}
