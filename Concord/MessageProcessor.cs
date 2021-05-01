using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;

namespace Automation.Concord
{
    /// <summary>
    /// Low level communicator interfaces with Automation Module serial port
    /// </summary>
    public class MessageProcessor
    {

        #region General

        bool stopped = true;

        ManualResetEvent outboundThreadRunning = new ManualResetEvent(true);
        ManualResetEvent inboundThreadRunning = new ManualResetEvent(true);

        Thread threadOutboundLoop = null;
        Thread threadInboundLoop = null;

        ManualResetEvent inboundResponseEvent = new ManualResetEvent(false);
        MessageResponse inboundResponse = MessageResponse.Waiting;

        ICommunicationDevice comm;

        MessageQueue outboundQueue = new MessageQueue(10);
        BlockingDeque<string> inboundQueue = new BlockingDeque<string>(10, true);

        MessageQueue outboundAckQueue = new MessageQueue(10);

        internal BlockingDeque<string> InboundQueue
        {
            get { return inboundQueue; }
            set { inboundQueue = value; }
        }

        public const int TIMEOUT_IMPLIED_NAK = 5000;

        public const int TIMEOUT_INBOUND_MESSAGE = 10000;
        public const int TIMEOUT_OUTBOUND_MESSAGE = 20000;

        public const int ERROR = -1;
        public const int CONTROL_CHAR_ACK = 0x06;
        public const int CONTROL_CHAR_NAK = 0x15;
        public const int CONTROL_CHAR_LF = 0x0A;

        #endregion

        #region Receiving


        private void InboundMessageLoop()
        {

            try
            {
                log.LogDebug(string.Format("{0} thread ({1}) started at {2}", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString()));

                inboundThreadRunning.Reset();

                int lastIndex;
                StringBuilder buffer;
                int index;
                int checksum;
                bool waitLf = false;

                int asciiCode;
                char c;

                while (!stopped)
                {
                    try
                    {
                        //command parsing loop

                        lastIndex = 0;
                        index = 0;
                        buffer = new StringBuilder(12);
                        checksum = 0;


                        while (!stopped)
                        {

                            asciiCode = comm.ReadChar();

                            if (asciiCode == ERROR)
                            {
                                // Communications device error while trying to read char
                                log.LogDebug("INBOUND ERROR read failure, delaying 500 ms");
                                Thread.Sleep(500);
                                continue;
                            }

                            // An ACK or NAK may be issued asynchronously with regard to any Outbound message 
                            // in progress.  This means that an ACK or NAK may occur in the middle of an Outbound message.
                            else if (asciiCode == CONTROL_CHAR_ACK)
                            {
                                // Do not add control characters to buffer
                                // Do not add control characters to buffer
                                inboundResponse = MessageResponse.ACK;
                                inboundResponseEvent.Set();

                                log.LogDebug("Received ACK");
                                continue;
                            }
                            else if (asciiCode == CONTROL_CHAR_NAK)
                            {
                                // Do not add control characters to buffer
                                inboundResponse = MessageResponse.NAK;
                                inboundResponseEvent.Set();

                                log.LogDebug("Received NAK");
                                continue;
                            }
                            else if (asciiCode == CONTROL_CHAR_LF)
                            {
                                // flag to wait for LF is only reset here
                                waitLf = false;

                                // reset parsing loop
                                break;
                            }
                            else if (waitLf)
                            {
                                // parsing loop has detected an error, wait for module to resend message
                                continue;
                            }

                            c = (char)asciiCode;
                            index++;

                            // last index preceeds message data
                            if (index <= 2)
                            {
                                buffer.Append(c);

                                // Last Index received
                                if (index == 2)
                                {
                                    try
                                    {
                                        lastIndex = Message.ToInt(buffer.ToString());
                                    }
                                    catch
                                    {
                                        //cannot parse last index message length
                                        goto badmessage;
                                    }
                                }
                            }
                            else if (lastIndex > 0 && index <= (lastIndex * 2))
                            {
                                // Message data 
                                buffer.Append(c);
                            }
                            else if (lastIndex > 0 && index <= (lastIndex * 2) + 2)
                            {

                                buffer.Append(c);

                                if (index < (lastIndex * 2) + 2)
                                {
                                    //still one byte remaining to be received
                                    continue;
                                }
                                else
                                {
                                    // End of message, checksum received
                                    try
                                    {
                                        checksum = Message.ToInt(buffer.ToString().Substring(index - 2));
                                    }
                                    catch (Exception ex)
                                    {
                                        log.LogError(ex, "Could not parse panel data checksum");

                                        // cannot parse checksum
                                        goto badmessage;
                                    }

                                    string lastIndexAndData = buffer.ToString().Substring(0, index - 2);
                                    int calculatedChecksum = Message.ToInt(Message.CalculateChecksum(lastIndexAndData));

                                    if (checksum != calculatedChecksum)
                                    {
                                        // message is not well formed
                                        goto badmessage;
                                    }
                                    else
                                    {
                                        // message successfully parsed and added to queue for processing. incoming message loop to send ACK
                                        // immediately send acknowledgement to prevent a replay
                                        SendAcknowledgement();


                                        log.LogDebug(string.Format("Received: {0} ({1})", buffer.ToString(), MessageCodeMap.CreateMessage(buffer.ToString()).ToString()));

                                        //queue message notification
                                        inboundQueue.Enqueue(buffer.ToString());

                                    }
                                }
                            }
                            else // (index > lastIndex)
                            {
                                // too many characters indicates bad message
                                goto badmessage;
                            }
                            continue;

                        badmessage:

                            log.LogDebug("Received bad message: " + buffer.ToString());

                            // ask for resend
                            SendNegativeAcknowledgement();

                            // ignore all characters until module receives NAK and resends message  
                            waitLf = true;
                            continue;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                    catch (ThreadInterruptedException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "INBOUND ERROR");

                        // infinite loop until closed
                        continue;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
            finally
            {
                log.LogDebug(string.Format("{0} thread ({1}) stopped at {2}", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString()));
                inboundThreadRunning.Set();
            }
        }


        #endregion

        #region Sending

        private void OutboundMessageLoop()
        {
            log.LogDebug(string.Format("{0} thread ({1}) started at {2}", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString()));

            outboundThreadRunning.Reset();

            try
            {
                while (!stopped)
                {
                    int failureCount = 0;

                    try
                    {
                        OutboundMessageRequest request = null;
                        bool timeout;
                        while (!stopped)
                        {
                            // loop here until a message is available
                            request = outboundQueue.Head(1000, out timeout);
                            if (!timeout) break;
                        }

                        if (stopped) return;

                        // check the expiration date
                        if (request.Deadline < DateTime.Now)
                        {
                            request.Response = MessageResponse.Expired;
                            try
                            {
                                request.WaitHandle.Set();
                            }
                            catch { }

                            // message is expired, remove message from queue
                            outboundQueue.Dequeue();

                            log.LogDebug("Send expired: " + request.Message.ToAsciiHex());

                            // loop next message
                            continue;
                        }


                        //send next message
                        string message = ((char)CONTROL_CHAR_LF).ToString() + request.Message.ToAsciiHex();

                        while (!stopped)
                        {
                            comm.WriteString(message);

                            // wait for response or timeout
                            bool signaled = inboundResponseEvent.WaitOne(TIMEOUT_IMPLIED_NAK);

                            if (!signaled)
                            {
                                request.Response = MessageResponse.Timeout;

                                log.LogDebug("Send timeout: " + request.Message.ToAsciiHex());
                                //timeout period expired
                                goto NAK;
                            }
                            else
                            {
                                // inbound loop recieved a response
                                if (inboundResponse == MessageResponse.ACK)
                                {
                                    request.Response = MessageResponse.ACK;
                                    try
                                    {
                                        request.WaitHandle.Set();
                                    }
                                    catch { }

                                    // message is acknowledged, remove message from queue
                                    outboundQueue.Dequeue();

                                    log.LogDebug("Sent: " + request.Message.ToAsciiHex() + "(" + request.Message.ToString() + ")");
                                    break;
                                }
                                else
                                {
                                    request.Response = MessageResponse.NAK;

                                    log.LogDebug("Send failed: " + request.Message.ToAsciiHex());
                                    goto NAK;
                                }
                            }


                        NAK:

                            // check expiration date before resend consideration
                            if (request.Deadline < DateTime.Now)
                            {
                                request.Response = MessageResponse.Expired;
                                try
                                {
                                    request.WaitHandle.Set();
                                }
                                catch { }

                                // message is expired, remove message from queue
                                outboundQueue.Dequeue();

                                log.LogDebug("Send expired: " + request.Message.ToAsciiHex());

                                break;
                            }

                            failureCount++;
                            if (failureCount > 10)
                            {
                                failureCount = 0;
                                Restart();
                                break;
                            }

                            log.LogDebug("Send retry: " + request.Message.ToAsciiHex());
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogDebug("OUTBOUND ERROR: " + ex.Message);
                    }
                }
            }
            finally
            {
                try
                {
                    log.LogDebug(string.Format("{0} thread ({1}) stopped at {2}", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString()));
                    outboundThreadRunning.Set();
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Exception occured exiting outbound message loop: " + ex.Message);
                }
            }
        }

        private void SendNegativeAcknowledgement()
        {
            if (stopped) return;
            log.LogDebug("Send NAK");
            comm.WriteChar(CONTROL_CHAR_NAK);
        }

        private void SendAcknowledgement()
        {
            if (stopped) return;
            log.LogDebug("Send ACK");
            comm.WriteChar(CONTROL_CHAR_ACK);
        }


        /// <summary>
        /// Synchronous method waits for queued commands to be sent or expired. 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <returns>Returns true if message is sent successfully, or false when message expires</returns>
        public MessageResponse SendMessage(Message message)
        {
            //if (stopped) throw new InvalidOperationException("Message processor is not running.");

            using (ManualResetEvent waitHandle = new ManualResetEvent(false))
            {
                OutboundMessageRequest request = new OutboundMessageRequest(message, waitHandle, DateTime.Now.AddMilliseconds(TIMEOUT_OUTBOUND_MESSAGE));

                // enqueue request and wait for response
                outboundQueue.Enqueue(request);
                //outboundQueue.InsertAtHead(request);

                bool signaled = request.WaitHandle.WaitOne(TIMEOUT_OUTBOUND_MESSAGE);
                if (!signaled)
                {
                    log.LogWarning("Panel processor send message timeout");
                    return MessageResponse.Expired;
                }
                else
                {
                    return request.Response;
                }
            }
        }

        #endregion

        #region Construction 


        /// The RS-232 Automation Module’s RS-232 port is configured as a DCE 
        /// device. Therefore it transmits data on pin 2, receives data on pin 3. 
        /// Pin 5 is the signal ground. All other pins are not used. If the 
        /// automation device’s RS-232 port is configured as a DTE port, then 
        /// a “straight through” DB-9 cable must be used. Otherwise a “null modem” 
        /// cable must be used.

        ///Set the communication parameters on the automation device’s RS-232 
        ///port as follows: 8 data bits, 9600 bps, odd parity, 1 stop bit.
        public MessageProcessor(ILogger logger)
        {
            this.log = logger;
        }

        ILogger log;

        /// <summary>
        /// Restart message processor
        /// </summary>
        public void Restart()
        {
            Stop();

            // wait a second
            try
            {
                Thread.Sleep(1000);
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (ThreadInterruptedException)
            {
                return;
            }

            //todo: evaluate why this was previously in a background thread ...
            //ThreadPool.QueueUserWorkItem(delegate
            //    {
            try
            {
                Start(null);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Exception occured restartging message processor: " + ex.Message);
            }
            //    }
            //);

        }

        /// <summary>
        /// Start message processing
        /// </summary>
        public void Start(ICommunicationDevice comm)
        {
            if (!stopped)
            {
                throw new InvalidOperationException("Message processor is already started.");
            }

            if (comm != null)
            {
                // save reference to method argument 
                this.comm = comm;
            }
            else if (this.comm == null)
            {
                // no parameter and communications device isn't configured
                throw new ArgumentException("Cannot start message processor without a communications device reference.", "comm");
            }

            stopped = false;
            log.LogInformation("Message processor starting at " + DateTime.Now.ToString());


            try
            {
                comm.Open();
            }
            catch (Exception ex)
            {
                try
                {
                    Stop(true);
                    log.LogError(ex, "Message processor stopped: Failed to open communications device");
                }
                catch { }
                return;
            }

            threadInboundLoop = new Thread(InboundMessageLoop);
            threadInboundLoop.IsBackground = false;
            threadInboundLoop.Name = "ProcessorInboundListener";
            threadInboundLoop.Priority = ThreadPriority.AboveNormal;
            threadInboundLoop.Start();

            threadOutboundLoop = new Thread(OutboundMessageLoop);
            threadOutboundLoop.IsBackground = false;
            threadOutboundLoop.Name = "ProcessorOutboundQueueWorker";
            threadOutboundLoop.Priority = ThreadPriority.AboveNormal;
            threadOutboundLoop.Start();


        }

        public void Stop()
        {
            Stop(true);
        }

        private void Stop(bool allowGracePeriod)
        {
            if (!this.stopped)
            {

                this.stopped = true;
                log.LogInformation("Message processor stopped at " + DateTime.Now.ToString());

                bool terminateThreads;

                if (allowGracePeriod)
                {
                    // wait for threads to finish
                    terminateThreads = !WaitHandle.WaitAll(new WaitHandle[] { outboundThreadRunning, inboundThreadRunning }, TIMEOUT_IMPLIED_NAK);
                }
                else
                {
                    terminateThreads = true;
                }

                // turn off outbound thread

                if (terminateThreads)
                {
                    try
                    {
                        // one or more of the threads is still running 
                        if (threadOutboundLoop != null && threadOutboundLoop.IsAlive)
                            threadOutboundLoop.Interrupt();

                        if (threadInboundLoop != null && threadInboundLoop.IsAlive)
                            threadInboundLoop.Interrupt();
                    }
                    catch { }
                }

                try
                {
                    if (comm != null)
                        comm.Close();
                }
                catch { }

                outboundThreadRunning.Set();
                inboundThreadRunning.Set();
            }
        }

        public bool IsRunning
        {
            get
            {
                return !stopped;
            }
        }
        #endregion
    }

   
    public delegate void MessageReceiver(MessageType type, string data);

}
