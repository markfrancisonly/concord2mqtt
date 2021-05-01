using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Automation.Concord
{
    [System.Diagnostics.DebuggerStepThrough]
    public class TcpServerCommunicationDevice : Automation.Concord.ICommunicationDevice
    {
        private NetworkStream clientStream;
        private IPEndPoint endPoint;
        private Thread listenerThread;
        private bool listening = false;
        private ILogger log;
        private TcpClient tcpClient;
        private TcpListener tcpListener;

        public TcpServerCommunicationDevice(ILogger logger, string hostName, int port)
        {
            log = logger;
            IPAddress address;
            if (!IPAddress.TryParse(hostName, out address)) throw new ArgumentException("hostName", "TCP communication end host name is invalid.");
            endPoint = new IPEndPoint(address, port);
        }

        public virtual void Close()
        {
            try
            {
                listening = false;
                tcpListener.Stop();

                try
                {
                    if (listenerThread != null && listenerThread.IsAlive)
                    {
                        if (!listenerThread.Join(1000))
                            listenerThread.Interrupt();
                    }
                }
                catch
                { }

                if (tcpClient == null) return;
                try
                {
                    tcpClient.Close();
                }
                catch { }

                tcpClient = null;
                clientStream = null;
            }
            catch
            {
                // eat
            }
        }

        public int GetReconnectDelay()
        {
            return 5 * 1000;
        }

        public void Open()
        {
            if (listening) return;

            try
            {
                listening = true;

                tcpListener = new TcpListener(IPAddress.Any, endPoint.Port);
                tcpListener.Start();

                listenerThread = new Thread(new ThreadStart(ListenForClients));
                listenerThread.IsBackground = false;
                listenerThread.Name = "TCP Listener";
                listenerThread.Start();
            }
            catch
            {
                throw;
            }
        }

        public virtual int ReadChar()
        {
            if (clientStream == null) return -1;
            return clientStream.ReadByte();
        }

        public virtual void WriteChar(int data)
        {
            if (!Connect()) return;
            clientStream.WriteByte((byte)data);
        }

        public virtual void WriteString(string data)
        {
            if (!Connect()) return;

            byte[] asciidata = Encoding.ASCII.GetBytes(data);
            clientStream.Write(asciidata, 0, asciidata.Length);
        }

        private bool Connect()
        {
            if (tcpClient == null) return false;
            return tcpClient.Connected;
        }

        private void ListenForClients()
        {
            while (listening)
            {
                try
                {
                    log.LogDebug("TCP server started, waiting for panel automation client");

                    //blocks until a client has connected to the server
                    TcpClient client = tcpListener.AcceptTcpClient();

                    IPEndPoint clientEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                    log.LogDebug(string.Format("TCP client connected from {0}", clientEndPoint));

                    if (clientEndPoint.Address.ToString() != endPoint.Address.ToString())
                    {
                        log.LogDebug("TCP client endpoint was rejected");
                        continue;
                    }

                    try
                    {
                        if (tcpClient != null)
                            tcpClient.Close();
                    }
                    catch { }

                    tcpClient = client;
                    tcpClient.ReceiveTimeout = 5000;

                    clientStream = tcpClient.GetStream();
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (ThreadInterruptedException)
                {
                    return;
                }
                catch { }

                try
                {
                    // minimum loop time
                    Thread.Sleep(100);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (ThreadInterruptedException)
                {
                    return;
                }
            }
        }
    }
}