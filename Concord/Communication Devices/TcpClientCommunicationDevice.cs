using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Automation.Concord
{
    public class TcpClientCommunicationDevice : Automation.Concord.ICommunicationDevice
    {
        private byte[] buffer;
        private Socket client;
        private IPEndPoint endPoint;

        public TcpClientCommunicationDevice(string hostName, int port)
        {
            buffer = new byte[1];

            IPAddress address;
            if (!IPAddress.TryParse(hostName, out address)) throw new ArgumentException("hostName", "TCP communication end host name is invalid.");
            endPoint = new IPEndPoint(address, port);

            Open();
        }

        public bool IsOpen
        {
            get
            {
                if (client == null) return false;

                // This is how you can determine whether a socket is still connected.
                bool blockingState = client.Blocking;
                try
                {
                    byte[] tmp = new byte[1];
                    client.Blocking = false;
                    client.Send(tmp, 0, 0);
                    return true;
                }
                catch (SocketException e)
                {
                    // 10035 == WSAEWOULDBLOCK
                    if (e.NativeErrorCode.Equals(10035))
                        return true;
                    else
                    {
                        return false;
                    }
                }
                finally
                {
                    client.Blocking = blockingState;
                }
            }
        }

        public virtual void Close()
        {
            try
            {
                if (client == null) return;
                client.Close();
            }
            catch
            {
                // eat
            }
        }

        public int GetReconnectDelay()
        {
            return 65 * 1000;
        }

        public void Open()
        {
            try
            {
                Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Don't allow another socket to bind to this port.
                tcpSocket.ExclusiveAddressUse = true;

                // Discards any pending data. For connection-oriented socket (TCP, for example),
                // Winsock resets the connection.
                tcpSocket.LingerState = new LingerOption(true, 0);

                // Disable the Nagle Algorithm for this tcp socket.
                tcpSocket.NoDelay = true;

                // Buffer one character at a time
                //tcpSocket.SendBufferSize = 1;

                // Set the timeout for synchronous receive methods
                tcpSocket.ReceiveTimeout = MessageProcessor.TIMEOUT_IMPLIED_NAK;
                tcpSocket.SendTimeout = MessageProcessor.TIMEOUT_IMPLIED_NAK;

                // Set the Time To Live (TTL) to 3 router hops.
                tcpSocket.Ttl = 3;

                client = tcpSocket;

                //client.Connect(endPoint);
            }
            catch
            {
                throw;
            }
        }

        public virtual int ReadChar()
        {
            Connect();
            try
            {
                client.Receive(buffer, 1, SocketFlags.None);
                return buffer[0];
            }
            catch
            {
                Close();
                return -1;
            }
        }

        public virtual void WriteChar(int data)
        {
            if (!Connect()) return;

            buffer[0] = (byte)data;
            client.Send(buffer);
        }

        public virtual void WriteString(string data)
        {
            if (!Connect()) return;

            byte[] asciidata = Encoding.ASCII.GetBytes(data);
            client.Send(asciidata);
        }

        private bool Connect()
        {
            if (client.Connected) return true;

            try
            {
                client.Connect(endPoint);
                return client.Connected;
            }
            catch
            {
                return false;
            }
        }
    }
}