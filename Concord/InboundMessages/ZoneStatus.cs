using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent whenever there is a change in zone state (e.g. trip, restore, alarm, cancel, 
    /// trouble, restoral, bypass, unbypass).  Also, if the Automation Module requests a Dynamic Data 
    /// Refresh Request this command will be sent for each zone that is not normal (i.e. any zone 
    /// that is open (non restored), in alarm, troubled or bypassed). The remote device should assume 
    /// that all zones are normal unless told otherwise.
    /// </summary>
   
    public class ZoneStatus : Message
    {
        //Format: 07h 21h [PN] [AN] [ZNh] [ZNl] [ZS] [CS]
        public ZoneStatus(string message) : base(message)
        { }

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
        }

        /// <summary>
        /// Zone 1-96
        /// </summary>
        public int Zone
        {
            get
            {
                string token = this[4];
                return ToInt(token);
            }
        }


        public ZoneState ZoneState
        {
            get
            {
                string token = this[5];
                return (ZoneState)ToInt(token);
            }
        }
    }



}
