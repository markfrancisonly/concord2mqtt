using System;

namespace Automation.Concord.InboundMessages.Alerts
{

   
    public class PartitionTest : AlarmTrouble
    {
        //Format: 0Dh 22h 02h [PN] [AN] [ST] [SNh] [SNm] [SNl] [GT] [ST] [ESh] [ESl] [CS]
        public PartitionTest(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Specific alert data type
        /// </summary>
        public PartitionTestType PartitionTestType
        {
            get
            {
                string token = this[9];
                return (PartitionTestType)ToInt(token);
            }
        }

        /// <summary>
        /// 0-252, if applicable
        /// </summary>
        public int User
        {
            get
            {
                return this.SpecificEventData;
            }
        }


    }
}
