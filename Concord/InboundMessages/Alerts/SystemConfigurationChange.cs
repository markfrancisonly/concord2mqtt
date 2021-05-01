using System;

namespace Automation.Concord.InboundMessages.Alerts
{

   
    public class SystemConfigurationChange : AlarmTrouble
    {
        //Format: 0Dh 22h 02h [PN] [AN] [ST] [SNh] [SNm] [SNl] [GT] [ST] [ESh] [ESl] [CS]
        public SystemConfigurationChange(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Specific alert data type
        /// </summary>
        public SystemConfigurationChangeType SystemConfigurationChangeType
        {
            get
            {
                string token = this[9];
                return (SystemConfigurationChangeType)ToInt(token);
            }
        }

    }
}
