using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent to stop any cadence being output.
    /// </summary>
   
    public class SirenStop : Message
    {
        //Format: 05h 22h 0Bh [PN] [AN] [CS]
        public SirenStop(string message) : base(message)
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
    }
}
