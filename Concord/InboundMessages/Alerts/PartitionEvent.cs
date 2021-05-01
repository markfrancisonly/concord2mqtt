using System;

namespace Automation.Concord.InboundMessages.Alerts
{

   
    public class PartitionEvent : AlarmTrouble
    {
        //Format: 0Dh 22h 02h [PN] [AN] [ST] [SNh] [SNm] [SNl] [GT] [ST] [ESh] [ESl] [CS]
        public PartitionEvent(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Specific alert data type
        /// </summary>
        public PartitionEventType PartitionEventType
        {
            get
            {
                string token = this[9];
                return (PartitionEventType)ToInt(token);
            }
        }

    }
}
