using System;

namespace Automation.Concord
{
    [Flags]
   
    public enum Feature
    {
        Chime = 1,
        EnergySaver = 2,
        NoDelay = 4,
        Latchkey = 8,
        SilentArming = 16,
        QuickArm = 32
    }
}
