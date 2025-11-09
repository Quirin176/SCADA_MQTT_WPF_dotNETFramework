using MQTT_Protocol.Internal;
using MQTT_Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static MQTT_Protocol.MQTT_Client;

namespace MQTT_Protocol
{
    public interface IMQTT_Client
    {
        event MqttMsgPublishEventHandler MQTTMsgPublishReceived;

        event MqttMsgPublishedEventHandler MQTTMsgPublished;

        event MqttMsgSubscribedEventHandler MQTTMsgSubscribed;

        event MqttMsgUnsubscribedEventHandler MQTTMsgUnsubscribed;

        event ConnectionClosedEventHandler ConnectionClosed;

        void Connect();
        byte Connect(string clientId);
        byte Connect(string clientId, string username, string password);
        byte Connect(string clientId, string username, string password, bool cleanSession, ushort keepAlivePeriod);
        byte Connect(string clientId, string username, string password, bool willRetain, byte willQosLevel, bool willFlag, string willTopic, string willMessage, bool cleanSession, ushort keepAlivePeriod);
        void Disconnect();
        ushort Subscribe(string[] topics, byte[] qosLevels);
        ushort Unsubscribe(string[] topics);
        ushort Publish(string topic, byte[] message);
        ushort Publish(string topic, byte[] message, byte qosLevel, bool retain);
    }
}
