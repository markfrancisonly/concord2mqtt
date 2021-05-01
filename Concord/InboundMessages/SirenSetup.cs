using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent whenever there is something to output with the interior siren output, status beeps, siren sounds, etc. There are 4 cadence bytes.  Each bit specifies a 125 mSec time slice, 1=on, 0=off, beginning with the most significant bit of the first cadence byte. The combined cadence is repeated RP times (if RP = 0, the cadence is repeated continuously).  The actual outputting of the cadence is not begun until either the Siren Go or the Siren Synchronize command is received (See commands below).  If alarm sirens are active, Advent also sends the Siren Setup in response to a Dynamic Data Refresh Request (20h).  
    /// </summary>
   
    public class SirenSetup : Message
    {
        //Format: 0Ah 22h 04h [PN] [AN] [RP] [CD1] [CD2] [CD3] [CD4] [CS]
        public SirenSetup(string message) : base(message)
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

        /// <summary>
        /// 1-255
        /// </summary>
        public int RepetitionCount
        {
            get
            {
                string token = this[4];
                return ToInt(token);
            }
        }

        public string Cadence
        {
            get
            {
                string token = string.Concat(this[5], this[6], this[7], this[8]);
                return token;
            }
        }

    }
}
