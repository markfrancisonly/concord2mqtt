using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent for each enabled partition, in response to an equipment list request from the Automation Module.
    /// </summary>
   
    public class EquipmentListPartition : Message
    {
        //Format: [LI] 04h [PN] [AN] [AL] [text] [CS]

        public EquipmentListPartition(string message) : base(message)
        { }


        /// <summary>
        /// Partition 1-6
        /// </summary>
        public int Partition
        {
            get
            {
                string token = this[1];
                return ToInt(token);
            }
        }

        public ArmingLevel ArmingLevelState
        {
            get
            {
                string token = this[3];
                return (ArmingLevel)ToInt(token);
            }
        }
    }
}
