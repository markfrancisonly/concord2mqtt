using System;

namespace Automation.Concord.InboundMessages.Alerts
{

   
    public class SystemTrouble : AlarmTrouble
    {
        //Format: 0Dh 22h 02h [PN] [AN] [ST] [SNh] [SNm] [SNl] [GT] [ST] [ESh] [ESl] [CS]
        public SystemTrouble(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Specific alert data type
        /// </summary>
        public SystemTroubleType SystemTroubleType
        {
            get
            {
                string token = this[9];
                return (SystemTroubleType)ToInt(token);
            }
        }

    }
}
