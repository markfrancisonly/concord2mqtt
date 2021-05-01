using System;

namespace Automation.Concord
{
   
    public enum DeviceCapability : int
    {

        PowerSupervision = 0x00,
        AccessControl = 0x01,
        AnalogSmoke = 0x02,
        AudioListenIn = 0x03,
        SnapCardSupervision = 0x04,
        Microburst = 0x05,
        DualPhoneLine = 0x06,
        EnergyManagement = 0x07,
        InputZones = 0x08,
        Phast_Automation_SystemManager = 0x09,
        PhoneInterface = 0x0A,
        RelayOutputs = 0x0B,
        RFReceiver = 0x0C,
        RFTransmitter = 0x0D,
        ParallelPrinter = 0x0E,
        Unknown = 0x0F,
        LEDTouchpad = 0x10,
        OneLine_TwoLine_BLTTouchpad = 0x11,
        GUITouchpad = 0x12,
        VoiceEvacuation = 0x13,
        Pager = 0x14,
        DownloadableCode_data = 0x15,
        JTECHPremisePager = 0x16,
        Cryptography = 0x17,
        LEDDisplay = 0x18
    }
}
