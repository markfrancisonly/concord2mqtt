using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent for each enrolled bus device, in response to an equipment list request from the Automation Module.
    /// </summary>
   
    public class EquipmentListSuperBusDevice : Message
    {
        //Format: [LI] 05h [PN] [AN] [ID1] [ID2] [ID3] [DS] [UN] [text] [CS]
        public EquipmentListSuperBusDevice(string message)
            : base(message)
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

        /// <summary>
        /// Device Unit ID
        /// </summary>
        public string DeviceUnitId
        {
            get
            {
                string token = string.Concat(this[2], this[3], this[4]);
                return token;
            }
        }

        public DeviceStatus DeviceStatus
        {
            get
            {
                string token = this[5];
                return (DeviceStatus)ToInt(token);
            }
        }

        /// <summary>
        /// 0-15
        /// </summary>
        public int Unit
        {
            get
            {
                string token = this[6];
                return ToInt(token);
            }

        }
    }
}
