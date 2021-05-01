namespace Automation.Concord
{
    internal class MessageQueue : BlockingDeque<OutboundMessageRequest>
    {
        public MessageQueue(int exponent)
            : base(exponent, false)
        { }

        public bool Contains(Message message)
        {
            lock (syncRoot)
            {
                string value = message.ToAsciiHex();

                int index = head;
                int i = length;
                MessageQueueItemComparer comparer = new MessageQueueItemComparer();
                while (i-- > 0)
                {
                    if (message == null)
                    {
                        if (buffer[index] == null)
                        {
                            return true;
                        }
                    }
                    else if ((buffer[index] != null) && comparer.Equals(buffer[index], message))
                    {
                        return true;
                    }
                    index = (index + 1) % N;
                }
                return false;
            }
        }

    }
}
