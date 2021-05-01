using System;
using System.Collections.Generic;

namespace Automation.Concord.Panel
{
    /// <summary>
    /// SuperBus Device
    /// </summary>
   
    public class Device
    {
        public Device()
        {
        }

        public Device(string deviceUnitId)
        {
            DeviceUnitId = deviceUnitId;
            Capabilities = new List<DeviceCapability>();
        }
        public string DeviceUnitId { get; set; }
        public List<DeviceCapability> Capabilities { get; set; }
        public DeviceStatus Status { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        public int ExtendedCapabilityData { get; set; }
    }
}
