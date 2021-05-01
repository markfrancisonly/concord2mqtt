using System;
using System.Reflection;
using System.Text;

namespace Automation.Concord
{

    /// <summary>
    /// Message class implements a generic Automation Module Protocol message. Subclass for more specific behavior or muliti-part messages.
    /// </summary>
   
    public abstract class Message
    {
        private const int MAXIMUM_MESSAGE_LENGTH = 57 * 2;

        #region State accessors
        /// <summary>
        /// Returns ASCII hex pair 
        /// </summary>
        /// <param name="index">Pair ordinal</param>
        /// <returns>Two ASCII characters to represent hex</returns>
        protected string this[int index]
        {
            get
            {
                return string.Concat(this.data[index * 2 + 0], this.data[index * 2 + 1]);
            }
            set
            {
                if (value.Length != 2) throw new ArgumentException();

                this.data[index * 2 + 0] = value[0];
                this.data[index * 2 + 1] = value[1];
            }
        }

        private int series = 0;

        public int Series
        {
            get { return series; }
            set { series = value; }
        }

        private StringBuilder data;

        protected string Data
        {
            get
            {
                return data.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException();
                if (value.Length < 2 || value.Length % 2 != 0) throw new ArgumentException();
                if (value.Length > MAXIMUM_MESSAGE_LENGTH) throw new ArgumentOutOfRangeException("Message size exceeded");

                data = new StringBuilder(value);
            }
        }

        /// <summary>
        /// Calculated message last index value
        /// </summary>
        protected string LastIndex
        {
            get
            {
                string lastIndex = CalculateLastIndex(Data);
                return lastIndex;
            }
        }

        /// <summary>
        /// Calculated message checksum value
        /// </summary>
        protected string Checksum
        {
            get
            {
                string checksum = CalculateChecksum(string.Concat(LastIndex, Data));
                return checksum;
            }

        }

        /// <summary>
        /// Parses the checksum from a valid message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static string ParseChecksum(string message)
        {
            return message.Substring(message.Length - 2);
        }

        /// <summary>
        /// Parses the lastindex from a valid message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static string ParseLastIndex(string message)
        {
            return message.Substring(0, 2);
        }

        /// <summary>
        /// Parses the data from a valid message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static string ParseData(string message)
        {
            return message.Substring(2, message.Length - 4);
        }

        #endregion

        #region Instance
        /// <summary>
        /// Creates an empty message instance
        /// </summary>
        protected Message()
        {
            this.data = new StringBuilder();
        }

        /// <summary>
        /// Instantiates a new message by checking syntax and parsing message data 
        /// </summary>
        /// <param name="command"></param>
        protected Message(string message)
        {
            if (!IsMessageValid(message))
            {
                throw new ArgumentException("Message is not well formed. Cannot rehydrate message object.", "message");
            }

            this.data = new StringBuilder(ParseData(message));
        }

        public string ToAsciiHex()
        {
            return string.Concat(LastIndex, Data, Checksum);
        }

        public override string ToString()
        {
            //return string.Concat(LastIndex, Data, Checksum);
            StringBuilder sb = new StringBuilder(this.GetType().Name);
            Type t = this.GetType();
            PropertyInfo[] pis = t.GetProperties();
            for (int i = 0; i < pis.Length; i++)
            {
                try
                {
                    PropertyInfo pi = (PropertyInfo)pis.GetValue(i);
                    sb.AppendFormat(":{0}", pi.GetValue(this, new object[] { }));
                }
                catch
                {
                    return string.Concat(LastIndex, Data, Checksum);
                }
            }

            return sb.ToString();
        }
        #endregion

        #region Static helpers
        /// <summary>
        /// Converts to Int32 from ASCII hex string
        /// </summary>
        /// <param name="asciiHex"></param>
        /// <returns></returns>
        public static int ToInt(string asciiHex)
        {
            return int.Parse(asciiHex, System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// Converts to ASCII hex string from Int32
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToAsciiHex(int value)
        {
            return String.Format("{0:X2}", value);
        }

        /// <summary>
        /// Calculates ASCII string representation of LastIndex hex value
        /// </summary>
        /// <param name="commandData">Data hex string</param>
        /// <returns>ASCII string of hex value</returns>
        public static string CalculateLastIndex(string data)
        {
            if (string.IsNullOrEmpty(data)) throw new ArgumentNullException("Cannot calculate LastIndex: Data is empty or null.");
            if (data.Length % 2 != 0) throw new ArgumentException("Cannot calculate LastIndex: Data length is not divisable by 2.", "command");

            string lastIndex = String.Format("{0:X2}", (data.Length / 2) + 1);

            return lastIndex;
        }

        /// <summary>
        /// Calculates ASCII string representation of checksum hex value. 
        /// </summary>
        /// <param name="commandLastIndexData">LastIndex and Data hex string</param>
        /// <returns>ASCII string of hex value</returns>
        public static string CalculateChecksum(string lastIndexAndData)
        {
            /*
             * A checksum is appended to each message.  It is the sum of the binary interpretation 
             * of all the preceding bytes in the message (control characters and ignored characters 
             * excluded), taken modulus 256.  The checksum is computed on the 8-bit binary 
             * representation of the ASCII pair, rather than the values of the individual 
             * ASCII characters.  
             * 
            */

            if (string.IsNullOrEmpty(lastIndexAndData)) throw new ArgumentNullException("Cannot calculate checksum: Data is empty or null.");
            if (lastIndexAndData.Length % 2 != 0) throw new ArgumentException("Cannot calculate checksum: Data length is not divisable by 2.");

            int sum = 0;
            for (int i = 0; i < lastIndexAndData.Length; i += 2)
            {

                string asciiPair = string.Concat(lastIndexAndData[i], lastIndexAndData[i + 1]);
                int pairValue = ToInt(asciiPair);

                sum += pairValue;
            }

            int checksum = sum % 256;

            string asciiString = ToAsciiHex(checksum);
            return asciiString;
        }

        /// <summary>
        /// First step in message validation. Checks command length and checksum for validity. 
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if syntax is valid</returns>
        public static bool IsMessageValid(string message)
        {
            if (string.IsNullOrEmpty(message) || message.Length < 6 || message.Length % 2 != 0 || message.Length > (MAXIMUM_MESSAGE_LENGTH + 4))
            {
                return false;
            }

            try
            {
                string lastIndex = ParseLastIndex(message);
                string data = ParseData(message);
                string checksum = ParseChecksum(message);

                if (lastIndex != CalculateLastIndex(data))
                {
                    return false;
                }
                else if (checksum != CalculateChecksum(lastIndex + data))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string[] Tokenize(string asciiHexString)
        {
            if ((asciiHexString.Length % 2) != 0)
                throw new ArgumentException("Invalid hex string");

            int startIndex = 0;
            int count = asciiHexString.Length / 2;

            string[] strArray = new string[count];
            for (int i = 0; i < count; i++)
            {
                strArray[i] = asciiHexString.Substring(startIndex, 2);
                startIndex += 2;
            }

            return strArray;

        }
        #endregion

    }
}
