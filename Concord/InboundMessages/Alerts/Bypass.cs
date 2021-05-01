using System;

namespace Automation.Concord.InboundMessages.Alerts
{

   
    public class Bypass : AlarmTrouble
    {
        //Format: 0Dh 22h 02h [PN] [AN] [ST] [SNh] [SNm] [SNl] [GT] [ST] [ESh] [ESl] [CS]
        public Bypass(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Specific alert data type
        /// </summary>
        public BypassType BypassType
        {
            get
            {
                string token = this[9];
                return (BypassType)ToInt(token);
            }
        }

    }
}
