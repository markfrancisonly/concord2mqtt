using System;

namespace Automation.Concord
{

    [Flags]
    public enum ScheduledAction
    {
        None = 0,
        Light1 = 1 << 0,
        Light2 = 1 << 1,
        Light3 = 1 << 2,
        Light4 = 1 << 3,
        Light5 = 1 << 4,
        Light6 = 1 << 5,
        Light7 = 1 << 6,
        Light8 = 1 << 7,
        Light9 = 1 << 8,
        Output1 = 1 << 9,
        Output2 = 1 << 10,
        Output3 = 1 << 11,
        Output4 = 1 << 12,
        Output5 = 1 << 13,
        Output6 = 1 << 14,
        LatchkeyOpen = 1 << 15,
        LatchkeyClose = 1 << 16,
        ExceptionOpen = 1 << 17,
        ExceptionClose = 1 << 18,
        AutoArmToLevel3 = 1 << 19,
        AllLights = Light1 | Light2 | Light3 | Light4 | Light5 | Light6 | Light7 | Light8 | Light9,
        AllOutputs = Output1 | Output2 | Output3 | Output4 | Output5 | Output6
    }
}
