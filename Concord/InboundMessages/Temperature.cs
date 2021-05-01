using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent in response to a Dynamic Data Refresh Request.
    /// </summary>
   
    public class Temperature : Message
    {
        //Format: 08h 22h 0Dh [PN] [AN] [TM] [ESL] [ESH] [CS]
        public Temperature(string message) : base(message)
        { }

        /// <summary>
        /// Partition 1-6
        /// </summary>
        public int Partition
        {
            get
            {
                string token = this[2];
                return ToInt(token);
            }
        }

        public int CurrentTemp
        {
            get
            {
                string token = this[4];
                return ToInt(token);
            }
        }

        public int EnergySaverLowTemp
        {
            get
            {
                string token = this[5];
                return ToInt(token);
            }
        }

        public int EnergySaverHighTemp
        {
            get
            {
                string token = this[6];
                return ToInt(token);
            }
        }

    }
}
