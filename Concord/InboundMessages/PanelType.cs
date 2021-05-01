using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent on panel power up initialization and when a communication 
    /// failure restoral with the Automation Module occurs.  Concord also sends the Panel Type in 
    /// response to a Dynamic Data Refresh Request (20h).  
    /// </summary>
   
    public class PanelType : Message
    {
        //Format: 0Bh 01h [PT] [HRh] [HRl] [SRh] [SRl] [SN4] [SN3] [SN2] [SN1] [CS]
        public PanelType(string message) : base(message)
        { }

        public string PanelTypeId
        {
            get
            {
                string token = this[1];
                return token;
            }
        }

        public string HardwareRevision
        {
            get
            {
                string token = string.Concat(this[2], this[3]);
                return token;
            }
        }

        public string SoftwareRevision
        {
            get
            {
                string token = string.Concat(this[4], this[5]);
                return token;
            }
        }

        public string SerialNumber
        {
            get
            {
                string token = string.Concat(this[6], this[7], this[8]);
                return token;
            }
        }
    }
}
