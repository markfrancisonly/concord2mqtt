using System;

namespace Automation.Concord
{
    [Flags]
   
    public enum ZoneState
    {
        Normal = 0,
        Opened = 1,
        Faulted = 2,
        Alarm = 4,
        Trouble = 8,
        Bypassed = 16
    }
}
