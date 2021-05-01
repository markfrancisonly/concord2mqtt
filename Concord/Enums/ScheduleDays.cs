using System;

namespace Automation.Concord
{
    [Flags]
   
    public enum ScheduleDays
    {
        NotSet = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64,
        Everyday = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday,
        Weekends = Saturday | Sunday,
        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday
    }
}
