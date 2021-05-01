using Automation.Concord.InboundMessages;
using System;
using System.Threading;

namespace Automation.Concord
{
    public class Emulator : MarshalByRefObject, ICommunicationDevice
    {
        #region Fields
        const int TIMEOUT_IMPLIED_NAK = 500;
        const int TIMEOUT_SEND = 2000;
        const int CONTROL_CHAR_ACK = 0x06;
        const int CONTROL_CHAR_NAK = 0x15;
        const int CONTROL_CHAR_LF = 0x0A;
        const int SIZE_OUTBOUND_BUFFER = 58;

        BlockingDeque<string> incomingQueue = new BlockingDeque<string>(1);
        int incomingCharIndex = 0;
        int controlCharacter = 0;
        #endregion

        #region Helpers
        public Emulator()
        {
            Open();
        }

        private void SendDisplayText(string text)
        {
            Message message;

            //Format: [LI] 22h 09h [PN] [AN] [MT] [DT] [CS]
            string data = "2209010001" + DisplayTextCodeMap.GetAsciiHexString(text);
            message = new PanelType(CompleteMessage(data));
            SendMessage(message);
        }
        private string CompleteMessage(string data)
        {
            string lastIndex = Message.CalculateLastIndex(data);
            return lastIndex + data + Message.CalculateChecksum(lastIndex + data);
        }
        #endregion

        #region ISerialPort
        public int ReadChar()
        {
            Thread.Sleep(1);
            int currentControlCharacter = controlCharacter;
            if (controlCharacter != 0)
            {
                controlCharacter = 0;
                return currentControlCharacter;
            }
            else
            {
                string nextMessage = incomingQueue.Head();
                return nextMessage[incomingCharIndex++];
            }
        }

        public void WriteString(string data)
        {
            throw new NotImplementedException();
        }

        public void WriteChar(int asciiCode)
        {
            Thread.Sleep(1);

            //if (text.Length == 1)
            //{
            if (asciiCode == CONTROL_CHAR_ACK)
            {
                //remove successful message
                incomingQueue.Dequeue();
                incomingCharIndex = 0;
            }
            else if (asciiCode == CONTROL_CHAR_NAK)
            {
                //restart current message
                incomingCharIndex = 0;

            }
            //}
            //else if (text.StartsWith(((char)CONTROL_CHAR_LF).ToString()))
            //{
            //    string message = text.Substring(1);
            //    if (Message.IsMessageValid(message))
            //    {
            //        SendAcknowledgement();
            //        MessageType messageType = MessageCodeMap.MapOutgoingProtocolMessage(message);

            //        switch (messageType)
            //        {
            //            case MessageType.DynamicDataRefreshRequest:
            //                ProcessDynamicDataRefresh();
            //                break;
            //            case MessageType.Keypress:
            //                ProcessKeypress(message);
            //                break;
            //            case MessageType.FullEquipmentListRequest:
            //                ProcessFullEquipmentListRequest();
            //                break;
            //            case MessageType.SingleEquipmentListRequest:
            //                ProcessSingleEquipementListRequest();
            //                break;
            //        }

            //    }
            //    else
            //    {
            //        SendNegativeAcknowledgement();
            //    }
            //}
        }

        public void Close()
        {
            isOpen = false;
        }

        public int GetReconnectDelay()
        {
            return 10 * 1000;
        }

        public bool IsOpen
        {
            get { return isOpen; }
        }
        private bool isOpen;

        public void Open()
        {
            isOpen = true;

            SendPanelType();
        }
        public string Status
        {
            get { return "Emulator"; }
        }
        #endregion

        #region Panel Emulation
        private void ProcessDynamicDataRefresh()
        {
            SendDisplayText("dynamic refresh received");
        }
        private void ProcessKeypress(string message)
        {
            OutboundMessages.Keypress keypress = new OutboundMessages.Keypress(message);
            string keys = TouchpadKeyCodeMap.GetString(keypress.KeyPresses);

            SendDisplayText(string.Format("keypress {0} received", keys));
        }
        private void ProcessFullEquipmentListRequest()
        {
            SendDisplayText("full equipment list received");
        }
        private void ProcessSingleEquipementListRequest()
        {
            SendDisplayText("single equipment list received");
        }

        private void SendPanelType()
        {
            Message message;

            //Format: 0Bh 01h [PT] [HRh] [HRl] [SRh] [SRl] [SN4] [SN3] [SN2] [SN1] [CS]
            string data = "01010000000000000000";
            message = new PanelType(CompleteMessage(data));
            SendMessage(message);
        }
        #endregion

        #region Module Emulator
        private void SendNegativeAcknowledgement()
        {
            if (controlCharacter != 0) throw new Exception("Compound nak condition");
            controlCharacter = CONTROL_CHAR_NAK;
        }

        private void SendAcknowledgement()
        {
            if (controlCharacter != 0) throw new Exception("Compound ack condition");
            controlCharacter = CONTROL_CHAR_ACK;
        }

        private void SendMessage(Message message)
        {
            string output = ((char)CONTROL_CHAR_LF).ToString() + message.ToAsciiHex();
            incomingQueue.Enqueue(output);
        }
        #endregion

    }
}
