using System;

namespace Automation.Concord.InboundMessages.Alerts
{

   
    public class OpeningClosing : AlarmTrouble
    {
        //Format: 0Dh 22h 02h [PN] [AN] [ST] [SNh] [SNm] [SNl] [GT] [ST] [ESh] [ESl] [CS]
        public OpeningClosing(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Specific alert data type
        /// </summary>
        public OpeningClosingType OpeningClosingType
        {
            get
            {
                string token = this[9];
                return (OpeningClosingType)ToInt(token);
            }
        }

    }
}
