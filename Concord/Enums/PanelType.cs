using System;
using System.Collections.Generic;
using System.Text;

namespace Automation.Concord.Panel
{
    public enum PanelType : int
    {
        Concord = 0x14,
        Concord_Express = 0x0b,
        Concord_Express_4 = 0x1e,
        Concord_Euro = 0x0e,
        Advent_Commercial_Fire_250 = 0x0d,
        Advent_Home_Navigator_132 = 0x0f,
        Advent_Commercial_Burg_250 = 0x10,
        Advent_Home_Navigator_250 = 0x11,
        Advent_Commercial_Burg_500 = 0x15,
        Advent_Commercial_Fire_500 = 0x16,
        Advent_Commercial_Fire_132 = 0x17,
        Advent_Commercial_Burg_132 = 0x18
    }
}
