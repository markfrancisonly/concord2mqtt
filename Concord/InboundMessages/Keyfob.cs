using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent whenever the panel receives a keypress from a keyfob.
    /// </summary>
   
    public class Keyfob : Message
    {

        // Format: 08h 23h 03h [PN] [AN] [ZNh] [ZNl] [KC] [CS]

        public Keyfob(string message)
            : base(message)
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
        /// Zone 1-96
        /// </summary>
        public int Zone
        {
            get
            {
                string token = this[5];
                return ToInt(token);
            }
        }

        /// <summary>
        /// Button pressed
        /// </summary>
        public KeyfobButton KeyCode
        {
            get
            {
                string token = this[6];
                return (KeyfobButton)ToInt(token);
            }

        }
    }




}

