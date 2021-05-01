using System;

namespace Automation.Concord
{

   
    public enum MessageType
    {
        Unknown,
        AlarmTrouble,
        ArmingLevel,
        AutomationEventLost,
        ClearAutomationDynamicImage,
        DynamicDataRefreshRequest,
        EntryExitDelay,
        EquipmentListComplete,
        EquipmentListLightToSensor,
        EquipmentListOutput,
        EquipmentListPartition,
        EquipmentListScheduledEvent,
        EquipmentListSchedule,
        EquipmentListSuperBusDevice,
        EquipmentListSuperBusDeviceCapabilities,
        EquipmentListUser,
        EquipmentListZone,
        FeatureState,
        FullEquipmentListRequest,
        Keyfob,
        Keypress,
        //Keypressed,
        LightsState,
        PanelType,
        Reserved,
        SingleEquipmentListRequest,
        SirenGo,
        SirenSetup,
        SirenStop,
        SirenSynchronize,
        Temperature,
        TimeAndDate,
        TouchpadDisplay,
        UserLights,
        ZoneStatus

    }

}
