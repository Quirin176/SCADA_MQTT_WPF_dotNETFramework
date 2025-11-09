using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Protocol
{
    public class MQTT_NetworkChannel : IMQTTNetworkChannel
    {
        private readonly RemoteCertificateValidationCallback userCertificateValidationCallback;

        private readonly LocalCertificateSelectionCallback userCertificateSelectionCallback;

        private string remoteHostName;

        private IPAddress remoteIpAddress;

        private int remotePort;

        private Socket socket;

        private bool secure;

        private X509Certificate caCert;

        private X509Certificate serverCert;

        private X509Certificate clientCert;

        private MQTT_SslProtocols sslProtocol;

        private SslStream sslStream;

        private NetworkStream netStream;

        public string RemoteHostName => remoteHostName;

        public IPAddress RemoteIpAddress => remoteIpAddress;

        public int RemotePort => remotePort;

        public bool DataAvailable
        {
            get
            {
                if (secure)
                {
                    return netStream.DataAvailable;
                }

                return socket.Available > 0;
            }
        }

        public MQTT_NetworkChannel(Socket socket)
            : this(socket, secure: false, null, MQTT_SslProtocols.None, null, null)
        {
        }

        public MQTT_NetworkChannel(Socket socket, bool secure, X509Certificate serverCert, MQTT_SslProtocols sslProtocol, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
        {
            this.socket = socket;
            this.secure = secure;
            this.serverCert = serverCert;
            this.sslProtocol = sslProtocol;
            this.userCertificateValidationCallback = userCertificateValidationCallback;
            this.userCertificateSelectionCallback = userCertificateSelectionCallback;
        }

        public MQTT_NetworkChannel(string remoteHostName, int remotePort)
            : this(remoteHostName, remotePort, secure: false, null, null, MQTT_SslProtocols.None, null, null)
        {
        }

        public MQTT_NetworkChannel(string remoteHostName, int remotePort, bool secure, X509Certificate caCert, X509Certificate clientCert, MQTT_SslProtocols sslProtocol, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
        {
            IPAddress iPAddress = null;
            try
            {
                iPAddress = IPAddress.Parse(remoteHostName);
            }
            catch
            {
            }

            if (iPAddress == null)
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(remoteHostName);
                if (hostEntry == null || hostEntry.AddressList.Length <= 0)
                {
                    throw new Exception("No address found for the remote host name");
                }

                int i;
                for (i = 0; hostEntry.AddressList[i] == null; i++)
                {
                }

                iPAddress = hostEntry.AddressList[i];
            }

            this.remoteHostName = remoteHostName;
            remoteIpAddress = iPAddress;
            this.remotePort = remotePort;
            this.secure = secure;
            this.caCert = caCert;
            this.clientCert = clientCert;
            this.sslProtocol = sslProtocol;
            this.userCertificateValidationCallback = userCertificateValidationCallback;
            this.userCertificateSelectionCallback = userCertificateSelectionCallback;
        }

        public void Connect()
        {
            socket = new Socket(remoteIpAddress.GetAddressFamily(), SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(remoteIpAddress, remotePort));
            if (secure)
            {
                netStream = new NetworkStream(socket);
                sslStream = new SslStream(netStream, leaveInnerStreamOpen: false, userCertificateValidationCallback, userCertificateSelectionCallback);
                X509CertificateCollection clientCertificates = null;
                if (clientCert != null)
                {
                    clientCertificates = new X509CertificateCollection(new X509Certificate[1] { clientCert });
                }

                sslStream.AuthenticateAsClient(remoteHostName, clientCertificates, MQTT_SslUtility.ToSslPlatformEnum(sslProtocol), checkCertificateRevocation: false);
            }
        }

        public int Send(byte[] buffer)
        {
            if (secure)
            {
                sslStream.Write(buffer, 0, buffer.Length);
                sslStream.Flush();
                return buffer.Length;
            }

            return socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        public int Receive(byte[] buffer)
        {
            if (secure)
            {
                int i = 0;
                int num = 0;
                for (; i < buffer.Length; i += num)
                {
                    num = sslStream.Read(buffer, i, buffer.Length - i);
                    if (num == 0)
                    {
                        return 0;
                    }
                }

                return buffer.Length;
            }

            int j = 0;
            int num2 = 0;
            for (; j < buffer.Length; j += num2)
            {
                num2 = socket.Receive(buffer, j, buffer.Length - j, SocketFlags.None);
                if (num2 == 0)
                {
                    return 0;
                }
            }

            return buffer.Length;
        }

        public int Receive(byte[] buffer, int timeout)
        {
            if (socket.Poll(timeout * 1000, SelectMode.SelectRead))
            {
                return Receive(buffer);
            }

            return 0;
        }

        public void Close()
        {
            if (secure)
            {
                netStream.Close();
                sslStream.Close();
            }

            socket.Close();
        }

        public void Accept()
        {
            if (secure)
            {
                netStream = new NetworkStream(socket);
                sslStream = new SslStream(netStream, leaveInnerStreamOpen: false, userCertificateValidationCallback, userCertificateSelectionCallback);
                sslStream.AuthenticateAsServer(serverCert, clientCertificateRequired: false, MQTT_SslUtility.ToSslPlatformEnum(sslProtocol), checkCertificateRevocation: false);
            }
        }
    }
}
