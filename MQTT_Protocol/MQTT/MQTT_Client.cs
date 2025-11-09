using MQTT_Protocol.Exceptions;
using MQTT_Protocol.Messages;
using MQTT_Protocol.Session;
using MQTT_Protocol.Utility;
using MQTT_Protocol.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trace = MQTT_Protocol.Utility.Trace;
using TraceLevel = MQTT_Protocol.Utility.TraceLevel;

namespace MQTT_Protocol
{
    public class MQTT_Client : IMQTT_Client
    {
        public delegate void MqttMsgPublishEventHandler(object sender, MQTTMsgPublishEventArgs e);

        public delegate void MqttMsgPublishedEventHandler(object sender, MQTTMsgPublishedEventArgs e);

        public delegate void MqttMsgSubscribedEventHandler(object sender, MQTTMsgSubscribedEventArgs e);

        public delegate void MqttMsgUnsubscribedEventHandler(object sender, MQTTMsgUnsubscribedEventArgs e);

        public delegate void ConnectionClosedEventHandler(object sender, EventArgs e);

        internal class MqttMsgContextFinder
        {
            internal ushort MessageId { get; set; }

            internal MQTTMsgFlow Flow { get; set; }

            internal MqttMsgContextFinder(ushort messageId, MQTTMsgFlow flow)
            {
                MessageId = messageId;
                Flow = flow;
            }

            internal bool Find(object item)
            {
                MQTTMsgContext mqttMsgContext = (MQTTMsgContext)item;
                if (mqttMsgContext.Message.Type == 3 && mqttMsgContext.Message.MessageId == MessageId)
                {
                    return mqttMsgContext.Flow == Flow;
                }

                return false;
            }
        }

        private string brokerHostName;

        private int brokerPort;

        private bool isRunning;

        private AutoResetEvent receiveEventWaitHandle;

        private AutoResetEvent inflightWaitHandle;

        private AutoResetEvent syncEndReceiving;

        private MQTTMsgBase msgReceived;

        private Exception exReceiving;

        private int keepAlivePeriod;

        private AutoResetEvent keepAliveEvent;

        private AutoResetEvent keepAliveEventEnd;

        private int lastCommTime;

        private IMQTTNetworkChannel channel;

        private Queue inflightQueue;

        private Queue internalQueue;

        private Queue eventQueue;

        private MQTT_ClientSession session;

        private MQTT_Settings settings;

        private ushort messageIdCounter;

        private bool isConnectionClosing;

        public bool IsConnected { get; private set; }

        public string ClientId { get; private set; }

        public bool CleanSession { get; private set; }

        public bool WillFlag { get; private set; }

        public byte WillQosLevel { get; private set; }

        public string WillTopic { get; private set; }

        public string WillMessage { get; private set; }

        public MQTT_ProtocolVersion ProtocolVersion { get; set; }

        public MQTT_Settings Settings => settings;

        public event MqttMsgPublishEventHandler MQTTMsgPublishReceived;

        public event MqttMsgPublishedEventHandler MQTTMsgPublished;

        public event MqttMsgSubscribedEventHandler MQTTMsgSubscribed;

        public event MqttMsgUnsubscribedEventHandler MQTTMsgUnsubscribed;

        public event ConnectionClosedEventHandler ConnectionClosed;

        [Obsolete("Use this ctor MqttClient(string brokerHostName) insted")]
        public MQTT_Client(IPAddress brokerIpAddress)
            : this(brokerIpAddress, 1883, secure: false, null, null, MQTT_SslProtocols.None)
        {
        }

        [Obsolete("Use this ctor MqttClient(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert) insted")]
        public MQTT_Client(IPAddress brokerIpAddress, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MQTT_SslProtocols sslProtocol)
        {
            Init(brokerIpAddress.ToString(), brokerPort, secure, caCert, clientCert, sslProtocol, null, null);
        }

        public MQTT_Client(string brokerHostName)
            : this(brokerHostName, 1883, secure: false, null, null, MQTT_SslProtocols.None)
        {
        }

        public MQTT_Client(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MQTT_SslProtocols sslProtocol)
        {
            Init(brokerHostName, brokerPort, secure, caCert, clientCert, sslProtocol, null, null);
        }

        public MQTT_Client(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MQTT_SslProtocols sslProtocol, RemoteCertificateValidationCallback userCertificateValidationCallback)
            : this(brokerHostName, brokerPort, secure, caCert, clientCert, sslProtocol, userCertificateValidationCallback, null)
        {
        }

        public MQTT_Client(string brokerHostName, int brokerPort, bool secure, MQTT_SslProtocols sslProtocol, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
            : this(brokerHostName, brokerPort, secure, null, null, sslProtocol, userCertificateValidationCallback, userCertificateSelectionCallback)
        {
        }

        public MQTT_Client(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MQTT_SslProtocols sslProtocol, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
        {
            Init(brokerHostName, brokerPort, secure, caCert, clientCert, sslProtocol, userCertificateValidationCallback, userCertificateSelectionCallback);
        }

        private void Init(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MQTT_SslProtocols sslProtocol, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
        {
            ProtocolVersion = MQTT_ProtocolVersion.Version_3_1_1;
            this.brokerHostName = brokerHostName;
            this.brokerPort = brokerPort;
            settings = MQTT_Settings.Instance;
            if (!secure)
            {
                settings.Port = this.brokerPort;
            }
            else
            {
                settings.SslPort = this.brokerPort;
            }

            syncEndReceiving = new AutoResetEvent(initialState: false);
            keepAliveEvent = new AutoResetEvent(initialState: false);
            inflightWaitHandle = new AutoResetEvent(initialState: false);
            inflightQueue = new Queue();
            receiveEventWaitHandle = new AutoResetEvent(initialState: false);
            eventQueue = new Queue();
            internalQueue = new Queue();
            session = null;
            channel = new MQTT_NetworkChannel(this.brokerHostName, this.brokerPort, secure, caCert, clientCert, sslProtocol, userCertificateValidationCallback, userCertificateSelectionCallback);
        }

        public void Connect()
        {

        }

        public byte Connect(string clientId)
        {
            return Connect(clientId, null, null, willRetain: false, 0, willFlag: false, null, null, cleanSession: true, 60);
        }

        public byte Connect(string clientId, string username, string password)
        {
            return Connect(clientId, username, password, willRetain: false, 0, willFlag: false, null, null, cleanSession: true, 60);
        }

        public byte Connect(string clientId, string username, string password, bool cleanSession, ushort keepAlivePeriod)
        {
            return Connect(clientId, username, password, willRetain: false, 0, willFlag: false, null, null, cleanSession, keepAlivePeriod);
        }

        public byte Connect(string clientId, string username, string password, bool willRetain, byte willQosLevel, bool willFlag, string willTopic, string willMessage, bool cleanSession, ushort keepAlivePeriod)
        {
            MQTTMsgConnect msg = new MQTTMsgConnect(clientId, username, password, willRetain, willQosLevel, willFlag, willTopic, willMessage, cleanSession, keepAlivePeriod, (byte)ProtocolVersion);
            try
            {
                channel.Connect();
            }
            catch (Exception innerException)
            {
                throw new MQTTConnectionException("Exception connecting to the broker", innerException);
            }

            lastCommTime = 0;
            isRunning = true;
            isConnectionClosing = false;
            Fx.StartThread(ReceiveThread);
            MQTTMsgConnack mqttMsgConnack = (MQTTMsgConnack)SendReceive(msg);
            if (mqttMsgConnack.ReturnCode == 0)
            {
                ClientId = clientId;
                CleanSession = cleanSession;
                WillFlag = willFlag;
                WillTopic = willTopic;
                WillMessage = willMessage;
                WillQosLevel = willQosLevel;
                this.keepAlivePeriod = keepAlivePeriod * 1000;
                RestoreSession();
                if (this.keepAlivePeriod != 0)
                {
                    Fx.StartThread(KeepAliveThread);
                }

                Fx.StartThread(DispatchEventThread);
                Fx.StartThread(ProcessInflightThread);
                IsConnected = true;
            }

            return mqttMsgConnack.ReturnCode;
        }

        public void Disconnect()
        {
            MQTTMsgDisconnect msg = new MQTTMsgDisconnect();
            Send(msg);
            OnConnectionClosing();
        }

        private void Close()
        {
            isRunning = false;
            if (receiveEventWaitHandle != null)
            {
                receiveEventWaitHandle.Set();
            }

            if (inflightWaitHandle != null)
            {
                inflightWaitHandle.Set();
            }

            keepAliveEvent.Set();
            if (keepAliveEventEnd != null)
            {
                keepAliveEventEnd.WaitOne();
            }

            inflightQueue.Clear();
            internalQueue.Clear();
            eventQueue.Clear();
            channel.Close();
            IsConnected = false;
        }

        private MQTTMsgPingResp Ping()
        {
            MQTTMsgPingReq msg = new MQTTMsgPingReq();
            try
            {
                return (MQTTMsgPingResp)SendReceive(msg, keepAlivePeriod);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(TraceLevel.Error, "Exception occurred: {0}", ex.ToString());
                OnConnectionClosing();
                return null;
            }
        }

        public ushort Subscribe(string[] topics, byte[] qosLevels)
        {
            MQTTMsgSubscribe mqttMsgSubscribe = new MQTTMsgSubscribe(topics, qosLevels);
            mqttMsgSubscribe.MessageId = GetMessageId();
            EnqueueInflight(mqttMsgSubscribe, MQTTMsgFlow.ToPublish);
            return mqttMsgSubscribe.MessageId;
        }

        public ushort Unsubscribe(string[] topics)
        {
            MQTTMsgUnsubscribe mqttMsgUnsubscribe = new MQTTMsgUnsubscribe(topics);
            mqttMsgUnsubscribe.MessageId = GetMessageId();
            EnqueueInflight(mqttMsgUnsubscribe, MQTTMsgFlow.ToPublish);
            return mqttMsgUnsubscribe.MessageId;
        }

        public ushort Publish(string topic, byte[] message)
        {
            return Publish(topic, message, 0, retain: false);
        }

        public ushort Publish(string topic, byte[] message, byte qosLevel, bool retain)
        {
            MQTTMsgPublish mqttMsgPublish = new MQTTMsgPublish(topic, message, dupFlag: false, qosLevel, retain);
            mqttMsgPublish.MessageId = GetMessageId();
            if (EnqueueInflight(mqttMsgPublish, MQTTMsgFlow.ToPublish))
            {
                return mqttMsgPublish.MessageId;
            }

            throw new MQTTClientException(MQTTClientErrorCode.InflightQueueFull);
        }

        private void OnInternalEvent(InternalEvent internalEvent)
        {
            lock (eventQueue)
            {
                eventQueue.Enqueue(internalEvent);
            }

            receiveEventWaitHandle.Set();
        }

        private void OnConnectionClosing()
        {
            if (!isConnectionClosing)
            {
                isConnectionClosing = true;
                receiveEventWaitHandle.Set();
            }
        }

        private void OnMqttMsgPublishReceived(MQTTMsgPublish publish)
        {
            if (this.MQTTMsgPublishReceived != null)
            {
                this.MQTTMsgPublishReceived(this, new MQTTMsgPublishEventArgs(publish.Topic, publish.Message, publish.DupFlag, publish.QosLevel, publish.Retain));
            }
        }

        private void OnMqttMsgPublished(ushort messageId, bool isPublished)
        {
            if (this.MQTTMsgPublished != null)
            {
                this.MQTTMsgPublished(this, new MQTTMsgPublishedEventArgs(messageId, isPublished));
            }
        }

        private void OnMqttMsgSubscribed(MQTTMsgSuback suback)
        {
            if (this.MQTTMsgSubscribed != null)
            {
                this.MQTTMsgSubscribed(this, new MQTTMsgSubscribedEventArgs(suback.MessageId, suback.GrantedQoSLevels));
            }
        }

        private void OnMqttMsgUnsubscribed(ushort messageId)
        {
            if (this.MQTTMsgUnsubscribed != null)
            {
                this.MQTTMsgUnsubscribed(this, new MQTTMsgUnsubscribedEventArgs(messageId));
            }
        }

        private void OnConnectionClosed()
        {
            if (this.ConnectionClosed != null)
            {
                this.ConnectionClosed(this, EventArgs.Empty);
            }
        }

        private void Send(byte[] msgBytes)
        {
            try
            {
                channel.Send(msgBytes);
                lastCommTime = Environment.TickCount;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(TraceLevel.Error, "Exception occurred: {0}", ex.ToString());
                throw new MQTTCommunicationException(ex);
            }
        }

        private void Send(MQTTMsgBase msg)
        {
            Trace.WriteLine(TraceLevel.Frame, "SEND {0}", msg);
            Send(msg.GetBytes((byte)ProtocolVersion));
        }

        private MQTTMsgBase SendReceive(byte[] msgBytes)
        {
            return SendReceive(msgBytes, 30000);
        }

        private MQTTMsgBase SendReceive(byte[] msgBytes, int timeout)
        {
            syncEndReceiving.Reset();
            try
            {
                channel.Send(msgBytes);
                lastCommTime = Environment.TickCount;
            }
            catch (Exception ex)
            {
                if (typeof(SocketException) == ex.GetType() && ((SocketException)ex).SocketErrorCode == SocketError.ConnectionReset)
                {
                    IsConnected = false;
                }

                Trace.WriteLine(TraceLevel.Error, "Exception occurred: {0}", ex.ToString());
                throw new MQTTCommunicationException(ex);
            }

            if (syncEndReceiving.WaitOne(timeout))
            {
                if (exReceiving == null)
                {
                    return msgReceived;
                }

                throw exReceiving;
            }

            throw new MQTTCommunicationException();
        }

        private MQTTMsgBase SendReceive(MQTTMsgBase msg)
        {
            return SendReceive(msg, 30000);
        }

        private MQTTMsgBase SendReceive(MQTTMsgBase msg, int timeout)
        {
            Trace.WriteLine(TraceLevel.Frame, "SEND {0}", msg);
            return SendReceive(msg.GetBytes((byte)ProtocolVersion), timeout);
        }

        private bool EnqueueInflight(MQTTMsgBase msg, MQTTMsgFlow flow)
        {
            bool flag = true;
            if (msg.Type == 3 && msg.QosLevel == 2)
            {
                lock (inflightQueue)
                {
                    MqttMsgContextFinder @object = new MqttMsgContextFinder(msg.MessageId, MQTTMsgFlow.ToAcknowledge);
                    MQTTMsgContext mqttMsgContext = new MQTTMsgContext();
                    if (mqttMsgContext != null)
                    {
                        mqttMsgContext.State = MQTTMsgState.QueuedQos2;
                        mqttMsgContext.Flow = MQTTMsgFlow.ToAcknowledge;
                        flag = false;
                    }
                }
            }

            if (flag)
            {
                MQTTMsgState state = MQTTMsgState.QueuedQos0;
                switch (msg.QosLevel)
                {
                    case 0:
                        state = MQTTMsgState.QueuedQos0;
                        break;
                    case 1:
                        state = MQTTMsgState.QueuedQos1;
                        break;
                    case 2:
                        state = MQTTMsgState.QueuedQos2;
                        break;
                }

                if (msg.Type == 8)
                {
                    state = MQTTMsgState.SendSubscribe;
                }
                else if (msg.Type == 10)
                {
                    state = MQTTMsgState.SendUnsubscribe;
                }

                MQTTMsgContext mqttMsgContext2 = new MQTTMsgContext();
                mqttMsgContext2.Message = msg;
                mqttMsgContext2.State = state;
                mqttMsgContext2.Flow = flow;
                mqttMsgContext2.Attempt = 0;
                MQTTMsgContext mqttMsgContext3 = mqttMsgContext2;
                lock (inflightQueue)
                {
                    flag = inflightQueue.Count < settings.InflightQueueSize;
                    if (flag)
                    {
                        inflightQueue.Enqueue(mqttMsgContext3);
                        Trace.WriteLine(TraceLevel.Queuing, "enqueued {0}", msg);
                        if (msg.Type == 3)
                        {
                            if (mqttMsgContext3.Flow == MQTTMsgFlow.ToPublish && (msg.QosLevel == 1 || msg.QosLevel == 2))
                            {
                                if (session != null)
                                {
                                    session.InflightMessages.Add(mqttMsgContext3.Key, mqttMsgContext3);
                                }
                            }
                            else if (mqttMsgContext3.Flow == MQTTMsgFlow.ToAcknowledge && msg.QosLevel == 2 && session != null)
                            {
                                session.InflightMessages.Add(mqttMsgContext3.Key, mqttMsgContext3);
                            }
                        }
                    }
                }
            }

            inflightWaitHandle.Set();
            return flag;
        }

        private void EnqueueInternal(MQTTMsgBase msg)
        {
            bool flag = true;
            if (msg.Type == 6)
            {
                lock (inflightQueue)
                {
                    MqttMsgContextFinder @object = new MqttMsgContextFinder(msg.MessageId, MQTTMsgFlow.ToAcknowledge);
                    MQTTMsgContext mqttMsgContext = new MQTTMsgContext();
                    if (mqttMsgContext == null)
                    {
                        MQTTMsgPubcomp mqttMsgPubcomp = new MQTTMsgPubcomp();
                        mqttMsgPubcomp.MessageId = msg.MessageId;
                        Send(mqttMsgPubcomp);
                        flag = false;
                    }
                }
            }
            else if (msg.Type == 7)
            {
                lock (inflightQueue)
                {
                    MqttMsgContextFinder object2 = new MqttMsgContextFinder(msg.MessageId, MQTTMsgFlow.ToPublish);
                    MQTTMsgContext mqttMsgContext2 = new MQTTMsgContext();
                    if (mqttMsgContext2 == null)
                    {
                        flag = false;
                    }
                }
            }
            else if (msg.Type == 5)
            {
                lock (inflightQueue)
                {
                    MqttMsgContextFinder object3 = new MqttMsgContextFinder(msg.MessageId, MQTTMsgFlow.ToPublish);
                    MQTTMsgContext mqttMsgContext3 = new MQTTMsgContext();
                    if (mqttMsgContext3 == null)
                    {
                        flag = false;
                    }
                }
            }

            if (flag)
            {
                lock (internalQueue)
                {
                    internalQueue.Enqueue(msg);
                    Trace.WriteLine(TraceLevel.Queuing, "enqueued {0}", msg);
                    inflightWaitHandle.Set();
                }
            }
        }

        private void ReceiveThread()
        {
            int num = 0;
            byte[] array = new byte[1];
            while (isRunning)
            {
                try
                {
                    num = channel.Receive(array);
                    if (num > 0)
                    {
                        switch ((byte)((array[0] & 0xF0) >> 4))
                        {
                            case 1:
                                throw new MQTTClientException(MQTTClientErrorCode.WrongBrokerMessage);
                            case 2:
                                msgReceived = MQTTMsgConnack.Parse(array[0], (byte)ProtocolVersion, channel);
                                Trace.WriteLine(TraceLevel.Frame, "RECV {0}", msgReceived);
                                syncEndReceiving.Set();
                                break;
                            case 12:
                                throw new MQTTClientException(MQTTClientErrorCode.WrongBrokerMessage);
                            case 13:
                                msgReceived = MQTTMsgPingResp.Parse(array[0], (byte)ProtocolVersion, channel);
                                Trace.WriteLine(TraceLevel.Frame, "RECV {0}", msgReceived);
                                syncEndReceiving.Set();
                                break;
                            case 8:
                                throw new MQTTClientException(MQTTClientErrorCode.WrongBrokerMessage);
                            case 9:
                                {
                                    MQTTMsgSuback mqttMsgSuback = MQTTMsgSuback.Parse(array[0], (byte)ProtocolVersion, channel);
                                    Trace.WriteLine(TraceLevel.Frame, "RECV {0}", mqttMsgSuback);
                                    EnqueueInternal(mqttMsgSuback);
                                    break;
                                }
                            case 3:
                                {
                                    MQTTMsgPublish mqttMsgPublish = MQTTMsgPublish.Parse(array[0], (byte)ProtocolVersion, channel);
                                    Trace.WriteLine(TraceLevel.Frame, "RECV {0}", mqttMsgPublish);
                                    EnqueueInflight(mqttMsgPublish, MQTTMsgFlow.ToAcknowledge);
                                    break;
                                }
                            case 4:
                                {
                                    MQTTMsgPuback mqttMsgPuback = MQTTMsgPuback.Parse(array[0], (byte)ProtocolVersion, channel);
                                    Trace.WriteLine(TraceLevel.Frame, "RECV {0}", mqttMsgPuback);
                                    EnqueueInternal(mqttMsgPuback);
                                    break;
                                }
                            case 5:
                                {
                                    MQTTMsgPubrec mqttMsgPubrec = MQTTMsgPubrec.Parse(array[0], (byte)ProtocolVersion, channel);
                                    Trace.WriteLine(TraceLevel.Frame, "RECV {0}", mqttMsgPubrec);
                                    EnqueueInternal(mqttMsgPubrec);
                                    break;
                                }
                            case 6:
                                {
                                    MQTTMsgPubrel mqttMsgPubrel = MQTTMsgPubrel.Parse(array[0], (byte)ProtocolVersion, channel);
                                    Trace.WriteLine(TraceLevel.Frame, "RECV {0}", mqttMsgPubrel);
                                    EnqueueInternal(mqttMsgPubrel);
                                    break;
                                }
                            case 7:
                                {
                                    MQTTMsgPubcomp mqttMsgPubcomp = MQTTMsgPubcomp.Parse(array[0], (byte)ProtocolVersion, channel);
                                    Trace.WriteLine(TraceLevel.Frame, "RECV {0}", mqttMsgPubcomp);
                                    EnqueueInternal(mqttMsgPubcomp);
                                    break;
                                }
                            case 10:
                                throw new MQTTClientException(MQTTClientErrorCode.WrongBrokerMessage);
                            case 11:
                                {
                                    MQTTMsgUnsuback mqttMsgUnsuback = MQTTMsgUnsuback.Parse(array[0], (byte)ProtocolVersion, channel);
                                    Trace.WriteLine(TraceLevel.Frame, "RECV {0}", mqttMsgUnsuback);
                                    EnqueueInternal(mqttMsgUnsuback);
                                    break;
                                }
                            case 14:
                                throw new MQTTClientException(MQTTClientErrorCode.WrongBrokerMessage);
                            default:
                                throw new MQTTClientException(MQTTClientErrorCode.WrongBrokerMessage);
                        }

                        exReceiving = null;
                    }
                    else
                    {
                        OnConnectionClosing();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(TraceLevel.Error, "Exception occurred: {0}", ex.ToString());
                    exReceiving = new MQTTCommunicationException(ex);
                    bool flag = false;
                    if (ex.GetType() == typeof(MQTTClientException))
                    {
                        MQTTClientException ex2 = ex as MQTTClientException;
                        flag = ex2.ErrorCode == MQTTClientErrorCode.InvalidFlagBits || ex2.ErrorCode == MQTTClientErrorCode.InvalidProtocolName || ex2.ErrorCode == MQTTClientErrorCode.InvalidConnectFlags;
                    }
                    else if (ex.GetType() == typeof(SocketException) || (ex.InnerException != null && ex.InnerException.GetType() == typeof(SocketException)))
                    {
                        flag = true;
                    }

                    if (flag)
                    {
                        OnConnectionClosing();
                    }
                }
            }
        }

        private void KeepAliveThread()
        {
            int num = 0;
            int millisecondsTimeout = keepAlivePeriod;
            keepAliveEventEnd = new AutoResetEvent(initialState: false);
            while (isRunning)
            {
                keepAliveEvent.WaitOne(millisecondsTimeout);
                if (isRunning)
                {
                    num = Environment.TickCount - lastCommTime;
                    if (num >= keepAlivePeriod)
                    {
                        Ping();
                        millisecondsTimeout = keepAlivePeriod;
                    }
                    else
                    {
                        millisecondsTimeout = keepAlivePeriod - num;
                    }
                }
            }

            keepAliveEventEnd.Set();
        }

        private void DispatchEventThread()
        {
            while (isRunning)
            {
                if (eventQueue.Count == 0 && !isConnectionClosing)
                {
                    receiveEventWaitHandle.WaitOne();
                }

                if (!isRunning)
                {
                    continue;
                }

                InternalEvent internalEvent = null;
                lock (eventQueue)
                {
                    if (eventQueue.Count > 0)
                    {
                        internalEvent = (InternalEvent)eventQueue.Dequeue();
                    }
                }

                if (internalEvent != null)
                {
                    MQTTMsgBase message = ((MsgInternalEvent)internalEvent).Message;
                    if (message != null)
                    {
                        switch (message.Type)
                        {
                            case 1:
                                throw new MQTTClientException(MQTTClientErrorCode.WrongBrokerMessage);
                            case 8:
                                throw new MQTTClientException(MQTTClientErrorCode.WrongBrokerMessage);
                            case 9:
                                OnMqttMsgSubscribed((MQTTMsgSuback)message);
                                break;
                            case 3:
                                if (internalEvent.GetType() == typeof(MsgPublishedInternalEvent))
                                {
                                    OnMqttMsgPublished(message.MessageId, isPublished: false);
                                }
                                else
                                {
                                    OnMqttMsgPublishReceived((MQTTMsgPublish)message);
                                }

                                break;
                            case 4:
                                OnMqttMsgPublished(message.MessageId, isPublished: true);
                                break;
                            case 6:
                                OnMqttMsgPublishReceived((MQTTMsgPublish)message);
                                break;
                            case 7:
                                OnMqttMsgPublished(message.MessageId, isPublished: true);
                                break;
                            case 10:
                                throw new MQTTClientException(MQTTClientErrorCode.WrongBrokerMessage);
                            case 11:
                                OnMqttMsgUnsubscribed(message.MessageId);
                                break;
                            case 14:
                                throw new MQTTClientException(MQTTClientErrorCode.WrongBrokerMessage);
                        }
                    }
                }

                if (eventQueue.Count == 0 && isConnectionClosing)
                {
                    Close();
                    OnConnectionClosed();
                }
            }
        }

        private void ProcessInflightThread()
        {
            MQTTMsgContext mqttMsgContext = null;
            MQTTMsgBase mqttMsgBase = null;
            MQTTMsgBase mqttMsgBase2 = null;
            InternalEvent internalEvent = null;
            bool flag = false;
            int num = -1;
            bool flag2 = false;
            try
            {
                while (isRunning)
                {
                    inflightWaitHandle.WaitOne(num);
                    if (!isRunning)
                    {
                        continue;
                    }

                    lock (inflightQueue)
                    {
                        flag2 = false;
                        flag = false;
                        mqttMsgBase2 = null;
                        num = int.MaxValue;
                        int num2 = inflightQueue.Count;
                        while (num2 > 0)
                        {
                            num2--;
                            flag = false;
                            mqttMsgBase2 = null;
                            if (!isRunning)
                            {
                                break;
                            }

                            mqttMsgContext = (MQTTMsgContext)inflightQueue.Dequeue();
                            mqttMsgBase = mqttMsgContext.Message;
                            switch (mqttMsgContext.State)
                            {
                                case MQTTMsgState.QueuedQos0:
                                    if (mqttMsgContext.Flow == MQTTMsgFlow.ToPublish)
                                    {
                                        Send(mqttMsgBase);
                                    }
                                    else if (mqttMsgContext.Flow == MQTTMsgFlow.ToAcknowledge)
                                    {
                                        internalEvent = new MsgInternalEvent(mqttMsgBase);
                                        OnInternalEvent(internalEvent);
                                    }

                                    Trace.WriteLine(TraceLevel.Queuing, "processed {0}", mqttMsgBase);
                                    break;
                                case MQTTMsgState.QueuedQos1:
                                case MQTTMsgState.SendSubscribe:
                                case MQTTMsgState.SendUnsubscribe:
                                    if (mqttMsgContext.Flow == MQTTMsgFlow.ToPublish)
                                    {
                                        mqttMsgContext.Timestamp = Environment.TickCount;
                                        mqttMsgContext.Attempt++;
                                        if (mqttMsgBase.Type == 3)
                                        {
                                            mqttMsgContext.State = MQTTMsgState.WaitForPuback;
                                            if (mqttMsgContext.Attempt > 1)
                                            {
                                                mqttMsgBase.DupFlag = true;
                                            }
                                        }
                                        else if (mqttMsgBase.Type == 8)
                                        {
                                            mqttMsgContext.State = MQTTMsgState.WaitForSuback;
                                        }
                                        else if (mqttMsgBase.Type == 10)
                                        {
                                            mqttMsgContext.State = MQTTMsgState.WaitForUnsuback;
                                        }

                                        Send(mqttMsgBase);
                                        num = ((settings.DelayOnRetry < num) ? settings.DelayOnRetry : num);
                                        inflightQueue.Enqueue(mqttMsgContext);
                                    }
                                    else if (mqttMsgContext.Flow == MQTTMsgFlow.ToAcknowledge)
                                    {
                                        MQTTMsgPuback mqttMsgPuback = new MQTTMsgPuback();
                                        mqttMsgPuback.MessageId = mqttMsgBase.MessageId;
                                        Send(mqttMsgPuback);
                                        internalEvent = new MsgInternalEvent(mqttMsgBase);
                                        OnInternalEvent(internalEvent);
                                        Trace.WriteLine(TraceLevel.Queuing, "processed {0}", mqttMsgBase);
                                    }

                                    break;
                                case MQTTMsgState.QueuedQos2:
                                    if (mqttMsgContext.Flow == MQTTMsgFlow.ToPublish)
                                    {
                                        mqttMsgContext.Timestamp = Environment.TickCount;
                                        mqttMsgContext.Attempt++;
                                        mqttMsgContext.State = MQTTMsgState.WaitForPubrec;
                                        if (mqttMsgContext.Attempt > 1)
                                        {
                                            mqttMsgBase.DupFlag = true;
                                        }

                                        Send(mqttMsgBase);
                                        num = ((settings.DelayOnRetry < num) ? settings.DelayOnRetry : num);
                                        inflightQueue.Enqueue(mqttMsgContext);
                                    }
                                    else if (mqttMsgContext.Flow == MQTTMsgFlow.ToAcknowledge)
                                    {
                                        MQTTMsgPubrec mqttMsgPubrec = new MQTTMsgPubrec();
                                        mqttMsgPubrec.MessageId = mqttMsgBase.MessageId;
                                        mqttMsgContext.State = MQTTMsgState.WaitForPubrel;
                                        Send(mqttMsgPubrec);
                                        inflightQueue.Enqueue(mqttMsgContext);
                                    }

                                    break;
                                case MQTTMsgState.WaitForPuback:
                                case MQTTMsgState.WaitForSuback:
                                case MQTTMsgState.WaitForUnsuback:
                                    {
                                        if (mqttMsgContext.Flow != 0)
                                        {
                                            break;
                                        }

                                        flag = false;
                                        lock (internalQueue)
                                        {
                                            if (internalQueue.Count > 0)
                                            {
                                                mqttMsgBase2 = (MQTTMsgBase)internalQueue.Peek();
                                            }
                                        }

                                        if (mqttMsgBase2 != null && ((mqttMsgBase2.Type == 4 && mqttMsgBase.Type == 3 && mqttMsgBase2.MessageId == mqttMsgBase.MessageId) || (mqttMsgBase2.Type == 9 && mqttMsgBase.Type == 8 && mqttMsgBase2.MessageId == mqttMsgBase.MessageId) || (mqttMsgBase2.Type == 11 && mqttMsgBase.Type == 10 && mqttMsgBase2.MessageId == mqttMsgBase.MessageId)))
                                        {
                                            lock (internalQueue)
                                            {
                                                internalQueue.Dequeue();
                                                flag = true;
                                                flag2 = true;
                                                Trace.WriteLine(TraceLevel.Queuing, "dequeued {0}", mqttMsgBase2);
                                            }

                                            internalEvent = ((mqttMsgBase2.Type != 4) ? new MsgInternalEvent(mqttMsgBase2) : new MsgPublishedInternalEvent(mqttMsgBase2, isPublished: true));
                                            OnInternalEvent(internalEvent);
                                            if (mqttMsgBase.Type == 3 && session != null && session.InflightMessages.ContainsKey(mqttMsgContext.Key))
                                            {
                                                session.InflightMessages.Remove(mqttMsgContext.Key);
                                            }

                                            Trace.WriteLine(TraceLevel.Queuing, "processed {0}", mqttMsgBase);
                                        }

                                        if (flag)
                                        {
                                            break;
                                        }

                                        int num3 = Environment.TickCount - mqttMsgContext.Timestamp;
                                        if (num3 >= settings.DelayOnRetry)
                                        {
                                            if (mqttMsgContext.Attempt < settings.AttemptsOnRetry)
                                            {
                                                mqttMsgContext.State = MQTTMsgState.QueuedQos1;
                                                inflightQueue.Enqueue(mqttMsgContext);
                                                num = 0;
                                            }
                                            else if (mqttMsgBase.Type == 3)
                                            {
                                                if (session != null && session.InflightMessages.ContainsKey(mqttMsgContext.Key))
                                                {
                                                    session.InflightMessages.Remove(mqttMsgContext.Key);
                                                }

                                                internalEvent = new MsgPublishedInternalEvent(mqttMsgBase, isPublished: false);
                                                OnInternalEvent(internalEvent);
                                            }
                                        }
                                        else
                                        {
                                            inflightQueue.Enqueue(mqttMsgContext);
                                            int num6 = settings.DelayOnRetry - num3;
                                            num = ((num6 < num) ? num6 : num);
                                        }

                                        break;
                                    }
                                case MQTTMsgState.WaitForPubrec:
                                    {
                                        if (mqttMsgContext.Flow != 0)
                                        {
                                            break;
                                        }

                                        flag = false;
                                        lock (internalQueue)
                                        {
                                            if (internalQueue.Count > 0)
                                            {
                                                mqttMsgBase2 = (MQTTMsgBase)internalQueue.Peek();
                                            }
                                        }

                                        if (mqttMsgBase2 != null && mqttMsgBase2.Type == 5 && mqttMsgBase2.MessageId == mqttMsgBase.MessageId)
                                        {
                                            lock (internalQueue)
                                            {
                                                internalQueue.Dequeue();
                                                flag = true;
                                                flag2 = true;
                                                Trace.WriteLine(TraceLevel.Queuing, "dequeued {0}", mqttMsgBase2);
                                            }

                                            MQTTMsgPubrel mqttMsgPubrel2 = new MQTTMsgPubrel();
                                            mqttMsgPubrel2.MessageId = mqttMsgBase.MessageId;
                                            mqttMsgContext.State = MQTTMsgState.WaitForPubcomp;
                                            mqttMsgContext.Timestamp = Environment.TickCount;
                                            mqttMsgContext.Attempt = 1;
                                            Send(mqttMsgPubrel2);
                                            num = ((settings.DelayOnRetry < num) ? settings.DelayOnRetry : num);
                                            inflightQueue.Enqueue(mqttMsgContext);
                                        }

                                        if (flag)
                                        {
                                            break;
                                        }

                                        int num3 = Environment.TickCount - mqttMsgContext.Timestamp;
                                        if (num3 >= settings.DelayOnRetry)
                                        {
                                            if (mqttMsgContext.Attempt < settings.AttemptsOnRetry)
                                            {
                                                mqttMsgContext.State = MQTTMsgState.QueuedQos2;
                                                inflightQueue.Enqueue(mqttMsgContext);
                                                num = 0;
                                                break;
                                            }

                                            if (session != null && session.InflightMessages.ContainsKey(mqttMsgContext.Key))
                                            {
                                                session.InflightMessages.Remove(mqttMsgContext.Key);
                                            }

                                            internalEvent = new MsgPublishedInternalEvent(mqttMsgBase, isPublished: false);
                                            OnInternalEvent(internalEvent);
                                        }
                                        else
                                        {
                                            inflightQueue.Enqueue(mqttMsgContext);
                                            int num5 = settings.DelayOnRetry - num3;
                                            num = ((num5 < num) ? num5 : num);
                                        }

                                        break;
                                    }
                                case MQTTMsgState.WaitForPubrel:
                                    if (mqttMsgContext.Flow != MQTTMsgFlow.ToAcknowledge)
                                    {
                                        break;
                                    }

                                    lock (internalQueue)
                                    {
                                        if (internalQueue.Count > 0)
                                        {
                                            mqttMsgBase2 = (MQTTMsgBase)internalQueue.Peek();
                                        }
                                    }

                                    if (mqttMsgBase2 != null && mqttMsgBase2.Type == 6)
                                    {
                                        if (mqttMsgBase2.MessageId == mqttMsgBase.MessageId)
                                        {
                                            lock (internalQueue)
                                            {
                                                internalQueue.Dequeue();
                                                flag2 = true;
                                                Trace.WriteLine(TraceLevel.Queuing, "dequeued {0}", mqttMsgBase2);
                                            }

                                            MQTTMsgPubcomp mqttMsgPubcomp = new MQTTMsgPubcomp();
                                            mqttMsgPubcomp.MessageId = mqttMsgBase.MessageId;
                                            Send(mqttMsgPubcomp);
                                            internalEvent = new MsgInternalEvent(mqttMsgBase);
                                            OnInternalEvent(internalEvent);
                                            if (mqttMsgBase.Type == 3 && session != null && session.InflightMessages.ContainsKey(mqttMsgContext.Key))
                                            {
                                                session.InflightMessages.Remove(mqttMsgContext.Key);
                                            }

                                            Trace.WriteLine(TraceLevel.Queuing, "processed {0}", mqttMsgBase);
                                        }
                                        else
                                        {
                                            inflightQueue.Enqueue(mqttMsgContext);
                                        }
                                    }
                                    else
                                    {
                                        inflightQueue.Enqueue(mqttMsgContext);
                                    }

                                    break;
                                case MQTTMsgState.WaitForPubcomp:
                                    {
                                        if (mqttMsgContext.Flow != 0)
                                        {
                                            break;
                                        }

                                        flag = false;
                                        lock (internalQueue)
                                        {
                                            if (internalQueue.Count > 0)
                                            {
                                                mqttMsgBase2 = (MQTTMsgBase)internalQueue.Peek();
                                            }
                                        }

                                        if (mqttMsgBase2 != null && mqttMsgBase2.Type == 7)
                                        {
                                            if (mqttMsgBase2.MessageId == mqttMsgBase.MessageId)
                                            {
                                                lock (internalQueue)
                                                {
                                                    internalQueue.Dequeue();
                                                    flag = true;
                                                    flag2 = true;
                                                    Trace.WriteLine(TraceLevel.Queuing, "dequeued {0}", mqttMsgBase2);
                                                }

                                                internalEvent = new MsgPublishedInternalEvent(mqttMsgBase2, isPublished: true);
                                                OnInternalEvent(internalEvent);
                                                if (mqttMsgBase.Type == 3 && session != null && session.InflightMessages.ContainsKey(mqttMsgContext.Key))
                                                {
                                                    session.InflightMessages.Remove(mqttMsgContext.Key);
                                                }

                                                Trace.WriteLine(TraceLevel.Queuing, "processed {0}", mqttMsgBase);
                                            }
                                        }
                                        else if (mqttMsgBase2 != null && mqttMsgBase2.Type == 5 && mqttMsgBase2.MessageId == mqttMsgBase.MessageId)
                                        {
                                            lock (internalQueue)
                                            {
                                                internalQueue.Dequeue();
                                                flag = true;
                                                flag2 = true;
                                                Trace.WriteLine(TraceLevel.Queuing, "dequeued {0}", mqttMsgBase2);
                                                inflightQueue.Enqueue(mqttMsgContext);
                                            }
                                        }

                                        if (flag)
                                        {
                                            break;
                                        }

                                        int num3 = Environment.TickCount - mqttMsgContext.Timestamp;
                                        if (num3 >= settings.DelayOnRetry)
                                        {
                                            if (mqttMsgContext.Attempt < settings.AttemptsOnRetry)
                                            {
                                                mqttMsgContext.State = MQTTMsgState.SendPubrel;
                                                inflightQueue.Enqueue(mqttMsgContext);
                                                num = 0;
                                                break;
                                            }

                                            if (session != null && session.InflightMessages.ContainsKey(mqttMsgContext.Key))
                                            {
                                                session.InflightMessages.Remove(mqttMsgContext.Key);
                                            }

                                            internalEvent = new MsgPublishedInternalEvent(mqttMsgBase, isPublished: false);
                                            OnInternalEvent(internalEvent);
                                        }
                                        else
                                        {
                                            inflightQueue.Enqueue(mqttMsgContext);
                                            int num4 = settings.DelayOnRetry - num3;
                                            num = ((num4 < num) ? num4 : num);
                                        }

                                        break;
                                    }
                                case MQTTMsgState.SendPubrel:
                                    if (mqttMsgContext.Flow == MQTTMsgFlow.ToPublish)
                                    {
                                        MQTTMsgPubrel mqttMsgPubrel = new MQTTMsgPubrel();
                                        mqttMsgPubrel.MessageId = mqttMsgBase.MessageId;
                                        mqttMsgContext.State = MQTTMsgState.WaitForPubcomp;
                                        mqttMsgContext.Timestamp = Environment.TickCount;
                                        mqttMsgContext.Attempt++;
                                        if (ProtocolVersion == MQTT_ProtocolVersion.Version_3_1 && mqttMsgContext.Attempt > 1)
                                        {
                                            mqttMsgPubrel.DupFlag = true;
                                        }

                                        Send(mqttMsgPubrel);
                                        num = ((settings.DelayOnRetry < num) ? settings.DelayOnRetry : num);
                                        inflightQueue.Enqueue(mqttMsgContext);
                                    }

                                    break;
                            }
                        }

                        if (num == int.MaxValue)
                        {
                            num = -1;
                        }

                        if (mqttMsgBase2 != null && !flag2)
                        {
                            internalQueue.Dequeue();
                            Trace.WriteLine(TraceLevel.Queuing, "dequeued {0} orphan", mqttMsgBase2);
                        }
                    }
                }
            }
            catch (MQTTCommunicationException ex)
            {
                if (mqttMsgContext != null)
                {
                    inflightQueue.Enqueue(mqttMsgContext);
                }

                Trace.WriteLine(TraceLevel.Error, "Exception occurred: {0}", ex.ToString());
                OnConnectionClosing();
            }
        }

        private void RestoreSession()
        {
            if (!CleanSession)
            {
                if (session != null)
                {
                    lock (inflightQueue)
                    {
                        foreach (MQTTMsgContext value in session.InflightMessages.Values)
                        {
                            inflightQueue.Enqueue(value);
                            if (value.Message.Type != 3 || value.Flow != 0)
                            {
                                continue;
                            }

                            if (value.Message.QosLevel == 1 && value.State == MQTTMsgState.WaitForPuback)
                            {
                                value.State = MQTTMsgState.QueuedQos1;
                            }
                            else if (value.Message.QosLevel == 2)
                            {
                                if (value.State == MQTTMsgState.WaitForPubrec)
                                {
                                    value.State = MQTTMsgState.QueuedQos2;
                                }
                                else if (value.State == MQTTMsgState.WaitForPubcomp)
                                {
                                    value.State = MQTTMsgState.SendPubrel;
                                }
                            }
                        }
                    }

                    inflightWaitHandle.Set();
                }
                else
                {
                    session = new MQTT_ClientSession(ClientId);
                }
            }
            else if (session != null)
            {
                session.Clear();
            }
        }

        private ushort GetMessageId()
        {
            messageIdCounter = (ushort)((messageIdCounter % 65535 == 0) ? 1 : ((ushort)(messageIdCounter + 1)));
            return messageIdCounter;
        }
    }
}
