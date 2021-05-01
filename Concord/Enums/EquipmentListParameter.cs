using System;

namespace Automation.Concord
{
   
    public enum EquipmentListParameter : int
    {
        ZoneData = 0x03,
        PartitionData = 0x04,
        SuperBusDeviceData = 0x05,
        SuperBusDeviceCapabilitiesData = 0x06,
        OutputData = 0x07,
        UserData = 0x09,
        ScheduleData = 0x0A,
        ScheduledEventData = 0x0B,
        LightToSensorAttachment = 0x0C
    }
}
