using System;

namespace Automation.Concord.InboundMessages.Alerts
{

   
    public class Trouble : AlarmTrouble
    {
        //Format: 0Dh 22h 02h [PN] [AN] [ST] [SNh] [SNm] [SNl] [GT] [ST] [ESh] [ESl] [CS]
        public Trouble(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Specific alert data type
        /// </summary>
        public TroubleType TroubleType
        {
            get
            {
                string token = this[9];
                return (TroubleType)ToInt(token);
            }
        }

    }
}
