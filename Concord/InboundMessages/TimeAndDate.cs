using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent in response to a Dynamic Data Refresh Request.
    /// </summary>
   
    public class TimeAndDate : Message
    {
        //Format: 08h 22h 0Eh [HR] [MN] [MM] [DD] [YY] [CS]
        public TimeAndDate(string message) : base(message)
        { }

        public int Hour
        {
            get
            {
                string token = this[2];
                return ToInt(token);
            }
        }

        public int Minute
        {
            get
            {
                string token = this[3];
                return ToInt(token);
            }
        }

        public int Month
        {
            get
            {
                string token = this[4];
                return ToInt(token);
            }
        }

        public int Day
        {
            get
            {
                string token = this[5];
                return ToInt(token);
            }
        }

        public int Year
        {
            get
            {
                string token = this[6];
                return ToInt(token);
            }
        }
    }
}
