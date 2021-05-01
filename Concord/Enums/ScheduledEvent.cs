using System;

namespace Automation.Concord
{
   
    public enum ScheduledEvent
    {
        Light1 = 1,
        Light2 = 2,
        Light3 = 3,
        Light4 = 4,
        Light5 = 5,
        Light6 = 6,
        Light7 = 7,
        Light8 = 8,
        Light9 = 9,
        Output1 = 0x71,
        Output2 = 0x72,
        Output3 = 0x73,
        Output4 = 0x74,
        Output5 = 0x75,
        Output6 = 0x76,
        LatchkeyOpen = 0xE0,
        LatchkeyClose = 0xE1,
        ExceptionOpen = 0xE2,
        ExceptionClose = 0xE3,
        AutoArmToLevel3 = 0xE5
    }
}
