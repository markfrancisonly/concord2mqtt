using System;

namespace Automation.Concord.InboundMessages.Alerts
{

   
    public class SystemEvent : AlarmTrouble
    {
        //Format: 0Dh 22h 02h [PN] [AN] [ST] [SNh] [SNm] [SNl] [GT] [ST] [ESh] [ESl] [CS]
        public SystemEvent(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Specific alert data type
        /// </summary>
        public SystemEventType SystemEventType
        {
            get
            {
                string token = this[9];
                return (SystemEventType)ToInt(token);
            }
        }

    }
}
