using System;
using System.Collections;

namespace Automation.Concord
{
    public class MessageQueueItemComparer : System.Collections.IEqualityComparer
    {
        private StringComparer comparer = StringComparer.InvariantCultureIgnoreCase;


        #region IEqualityComparer Members
        internal bool Equals(OutboundMessageRequest request, Message y)
        {
            return this.Equals(request.Message, y);
        }

        public bool Equals(Message x, Message y)
        {
            string messageStringA = x.ToAsciiHex();
            string messageStringB = y.ToAsciiHex();

            bool sameMessage = comparer.Equals(messageStringA, messageStringB);
            if (sameMessage)
            {
                if (x.Series == y.Series)
                    return true;
            }

            return false;
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            Message messageX = x as Message;
            Message messageY = y as Message;
            if (x == null || y == null)
                throw new ArgumentException();

            return this.Equals(messageX, messageY);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            Message message = obj as Message;
            if (message == null)
                throw new ArgumentException();

            string str = message.ToAsciiHex() + message.Series.ToString();
            return comparer.GetHashCode(str);
        }

        #endregion
    }


}
