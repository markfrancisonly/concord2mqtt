using System;
using System.Threading;

namespace Automation.Concord
{
    internal class OutboundMessageRequest
    {
        protected const int CONTROL_CHAR_ACK = 0x06;
        protected const int CONTROL_CHAR_NAK = 0x15;
        protected const int CONTROL_CHAR_LF = 0x0A;

        public Message Message;
        public MessageResponse Response = MessageResponse.Waiting;

        //public int RemainingRetries = 4;

        /// <summary>
        /// Time after which message will not be sent
        /// </summary>
        public DateTime Deadline;

        /// <summary>
        /// Next message will not be processed until event handle is reset or timeout occurs
        /// </summary>
        public EventWaitHandle WaitHandle;

        /// <summary>
        /// Creates a message request with defined expiration 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="waitHandle"></param>
        /// <param name="expiry">Box allows a group of messages to be expired simulateously</param>
        public OutboundMessageRequest(Message message, EventWaitHandle waitHandle, DateTime deadline)
        {
            Message = message;
            WaitHandle = waitHandle;
            Deadline = deadline;
        }
    }


}


