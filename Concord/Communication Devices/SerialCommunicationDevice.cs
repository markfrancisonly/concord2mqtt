using RJCP.IO.Ports;
using System;
using System.Text;
using System.Threading;

namespace Automation.Concord
{
    [System.Diagnostics.DebuggerStepThrough]
    public class SerialCommunicationDevice : MarshalByRefObject, Automation.Concord.ICommunicationDevice
    {
        protected SerialPortStream serialPort;
        private char[] buffer;
        private string status = "";

        /// <summary>
        /// Defaults to COM1
        /// </summary>
        public SerialCommunicationDevice()
            : this("COM1")
        {
        }

        public SerialCommunicationDevice(string portName)
        {
            buffer = new char[1];
            ConfigureSerialPort(portName);
        }

        public bool IsOpen
        {
            get
            {
                if (serialPort == null)
                    return false;
                else
                    return serialPort.IsOpen;
            }
        }

        public virtual string Status
        {
            get
            {
                return status;
            }
        }

        public virtual void Close()
        {
            if (serialPort == null) return;
            serialPort.Close();
        }

        public void ConfigureSerialPort(string portName)
        {
            portName = portName.ToUpper();

            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    if (serialPort.PortName == portName)
                        return;
                    else
                        serialPort.Close();
                }
            }
            catch
            { //dont know, dont care
            }

            SerialPortStream port;
            port = new SerialPortStream(portName, 9600, 8, Parity.Odd, StopBits.One);

            if (port.IsOpen)
            {
                port.Close();
            }

            // Set the read/write timeouts
            port.ReadTimeout = Timeout.Infinite;
            port.WriteTimeout = Timeout.Infinite;

            port.WriteBufferSize = 20;
            port.ReadBufferSize = 20;

            serialPort = port;

            status = portName;
        }

        public int GetReconnectDelay()
        {
            return 10 * 1000;
        }

        public void Open()
        {
            try
            {
                if (serialPort == null)
                    throw new Exception("Serial port has not been initalized.");

                if (serialPort.IsOpen)
                    return;
                else
                {
                    serialPort.Open();
                }
            }
            catch (System.InvalidOperationException)
            {
                // The specified port is open.
                //if (comport.IsOpen) comport.Close();
                throw;
            }
            catch (System.IO.IOException)
            {
                // The port is in an invalid state. - or - An attempt to set the state of the
                // underlying port failed. For example, the parameters passed from this System.IO.Ports.SerialPort
                // object were invalid.
                throw;
            }
            catch (System.UnauthorizedAccessException)
            {
                // Access is denied to the port.
                throw;
            }
        }

        public virtual int ReadChar()
        {
            WaitForOpenPort();
            // loop until a character is read
            return serialPort.ReadByte();
        }

        public virtual void WriteChar(int data)
        {
            WaitForOpenPort();
            buffer[0] = (char)data;

            serialPort.Write(buffer, 0, 1);
        }

        public void WriteString(string data)
        {
            WaitForOpenPort();
            byte[] asciidata = Encoding.ASCII.GetBytes(data);
            serialPort.Write(asciidata, 0, asciidata.Length);
        }

        protected virtual void WaitForOpenPort()
        {
            while (!serialPort.IsOpen)
            {
                try
                {
                    Open();
                    break;
                }
                catch
                {
                    try
                    {
                        Thread.Sleep(500);
                    }
                    catch (System.Threading.ThreadInterruptedException)
                    {
                        return;
                    }
                }
            }
        }
    }
}