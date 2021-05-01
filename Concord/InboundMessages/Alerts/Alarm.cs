using System;

namespace Automation.Concord.InboundMessages.Alerts
{

   
    public class Alarm : AlarmTrouble
    {
        //Format: 0Dh 22h 02h [PN] [AN] [ST] [SNh] [SNm] [SNl] [GT] [ST] [ESh] [ESl] [CS]
        public Alarm(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Specific alert data type
        /// </summary>
        public AlarmType AlarmType
        {
            get
            {
                string token = this[9];
                return (AlarmType)ToInt(token);
            }
        }

    }
}
