using System;

using System.Text;

namespace Automation.Concord.OutboundMessages
{
   
    public class Keypress : Message
    {
        //Format: [LI] 40h [PN] [AN] [KP1] … [KPn] [CS]
        private const string HEADER = "40";

        public Keypress(string message) : base(message) { }


        /// <summary>
        /// Send key presses to specified partition
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="keys"></param>
        public Keypress(int partition, params TouchpadKey[] keys)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(HEADER);
            builder.Append(ToAsciiHex(partition));
            builder.Append("00"); // area is 0 for concord

            if (keys.Length >= 55) throw new ArgumentOutOfRangeException();

            foreach (TouchpadKey key in keys)
            {

                builder.Append(Message.ToAsciiHex((int)key));
            }

            this.Data = builder.ToString();
        }

        /// <summary>
        /// Send key presses to specified partition
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="numbersPoundStar">String consisting of 0-9, #, or *</param>
        public Keypress(int partition, string numbersPoundStar) : this(partition, TouchpadKeyCodeMap.GetKeypress(numbersPoundStar)) { }

        /// <summary>
        /// Partition 1-6
        /// </summary>
        public int Partition
        {
            get
            {
                string token = this[1];
                return ToInt(token);
            }
            set
            {
                if (value < 1 || value > 6) throw new ArgumentOutOfRangeException();

                this[1] = ToAsciiHex(value);
            }
        }

        public TouchpadKey[] KeyPresses
        {
            get
            {
                const int offset = 3;

                TouchpadKey[] array = new TouchpadKey[(this.Data.Length - (offset * 2)) / 2];

                for (int i = 0; i < array.Length; i++)
                {
                    string token = this[i + offset];
                    array[i] = (TouchpadKey)ToInt(token);
                }

                return array;
            }
        }

    }


}
