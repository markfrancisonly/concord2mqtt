using System;

namespace Automation.Concord.InboundMessages
{
   
    public class EquipmentListSuperBusDeviceCapabilities : Message
    {
        //Format: 07h 06h [ID1] [ID2] [ID3] [CN] [CD] [CS]

        public EquipmentListSuperBusDeviceCapabilities(string message)
            : base(message)
        { }

        /// <summary>
        /// Device Unit ID
        /// </summary>
        public string DeviceUnitId
        {
            get
            {
                string token = string.Concat(this[1], this[2], this[3]);
                return token;
            }
        }

        public DeviceCapability Capability
        {
            get
            {
                string token = this[4];
                return (DeviceCapability)ToInt(token);
            }
        }

        /// <summary>
        /// Optional
        /// </summary>
        public int ExtendedCapabilityData
        {
            get
            {
                string token = this[5];
                return ToInt(token);
            }
        }
    }
}
