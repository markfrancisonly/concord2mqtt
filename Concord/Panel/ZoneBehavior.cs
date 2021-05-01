using System.Collections.Generic;

namespace Automation.Concord.Panel
{
    /// <summary>
    /// Behavior of zone determined by zone's group
    /// </summary>
    public struct ZoneBehavior
    {
        static List<ZoneBehavior?> list;
        static ZoneBehavior()
        {
            list = new List<ZoneBehavior?>();

            //   Group, Defintion, Delay, Arm level, Report, Supervisory, Restoral, Alarm
            list.Add(new ZoneBehavior(0, "Fixed panic", ZoneGroupDelay.Instant, true, true, true, true, true, false, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(1, "Portable panic", ZoneGroupDelay.Instant, true, true, true, true, false, false, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(2, "Fixed panic", ZoneGroupDelay.Instant, true, true, true, true, true, false, ZoneAlarm.Silent));
            list.Add(new ZoneBehavior(3, "Portable panic", ZoneGroupDelay.Instant, true, true, true, true, false, false, ZoneAlarm.Silent));
            list.Add(new ZoneBehavior(4, "Fixed auxiliary", ZoneGroupDelay.Instant, true, true, true, true, true, false, ZoneAlarm.Auxiliary));
            list.Add(new ZoneBehavior(5, "Fixed auxiliary siren signals report", ZoneGroupDelay.Instant, true, true, true, true, true, false, ZoneAlarm.Auxiliary));
            list.Add(new ZoneBehavior(6, "Portable auxiliary", ZoneGroupDelay.Instant, true, true, true, true, false, false, ZoneAlarm.Auxiliary));
            list.Add(new ZoneBehavior(7, "Portable auxiliary siren signals report", ZoneGroupDelay.Instant, true, true, true, true, false, false, ZoneAlarm.Auxiliary));
            list.Add(new ZoneBehavior(8, "Special intrusion instant", ZoneGroupDelay.Instant, true, true, true, true, true, true, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(9, "Special intrusion", ZoneGroupDelay.Standard, true, true, true, true, true, true, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(10, "Entry/exit delay", ZoneGroupDelay.Standard, false, true, true, true, true, true, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(11, "Entry/exit delay Extended", ZoneGroupDelay.Extended, false, true, true, true, true, true, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(12, "Entry/exit delay", ZoneGroupDelay.TwiceExtended, false, true, true, true, true, true, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(13, "Perimeter", ZoneGroupDelay.Standard, false, true, true, true, true, true, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(14, "Instant interior door", ZoneGroupDelay.Follower, false, true, true, true, true, true, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(15, "Instant interior motion", ZoneGroupDelay.Follower, false, true, true, true, true, false, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(16, "Instant interior cross zone motion", ZoneGroupDelay.Follower, false, false, true, true, true, true, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(17, "Instant interior", ZoneGroupDelay.Follower, false, false, true, true, true, false, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(18, "Instant interior", ZoneGroupDelay.Follower, false, false, true, true, true, false, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(19, "Delayed interior", ZoneGroupDelay.Interior, false, false, true, true, true, true, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(20, "Delayed interior", ZoneGroupDelay.Standard, false, false, true, true, true, false, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(21, "Local instant interior ", ZoneGroupDelay.Instant, true, true, true, false, true, true, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(22, "Local delayed interior", ZoneGroupDelay.Standard, true, true, true, false, true, true, ZoneAlarm.Police));
            list.Add(new ZoneBehavior(23, "Local instant auxiliary", ZoneGroupDelay.Instant, true, true, true, false, true, true, ZoneAlarm.Auxiliary));
            list.Add(new ZoneBehavior(24, "Local instant auxiliary", ZoneGroupDelay.Instant, true, true, true, false, true, true, ZoneAlarm.Auxiliary));
            list.Add(new ZoneBehavior(25, "Local special chime", ZoneGroupDelay.Instant, true, true, true, false, true, false, ZoneAlarm.SpecialChime));
            list.Add(new ZoneBehavior(26, "Fire", ZoneGroupDelay.Instant, true, true, true, true, true, true, ZoneAlarm.Fire));
            list.Add(new ZoneBehavior(27, "Output module", ZoneGroupDelay.Instant, true, true, true, false, true, true, ZoneAlarm.Silent));
            list.Add(new ZoneBehavior(28, "Output module", ZoneGroupDelay.Instant, true, true, true, false, true, false, ZoneAlarm.Silent));
            list.Add(new ZoneBehavior(29, "Auxiliary", ZoneGroupDelay.Instant, true, true, true, true, true, true, ZoneAlarm.Auxiliary));
            list.Add(null); //30
            list.Add(null); //31
            list.Add(new ZoneBehavior(32, "Output Module", ZoneGroupDelay.Instant, true, true, true, false, false, false, ZoneAlarm.Silent));
            list.Add(new ZoneBehavior(33, "Siren", ZoneGroupDelay.Instant, true, true, true, true, true, false, ZoneAlarm.Silent));
            list.Add(new ZoneBehavior(34, "Gas", ZoneGroupDelay.Instant, true, true, true, true, true, true, ZoneAlarm.Auxiliary));
            list.Add(new ZoneBehavior(35, "Local instant police (day zone)", ZoneGroupDelay.Instant, true, true, true, true, true, true, ZoneAlarm.Police));
            list.Add(null); //36
            list.Add(null); //37
            list.Add(new ZoneBehavior(38, "Auxiliary", ZoneGroupDelay.Instant, true, true, true, true, true, true, ZoneAlarm.Auxiliary));
        }

        /// <summary>
        /// Returns effective behavior for zone. Null if zone group is indeterminate or otherwise out of range.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public static ZoneBehavior? GetZoneBehavior(Zone zone)
        {
            if (zone.Group == null) return null;

            int index = (int)zone.Group.Value;
            if (index < 0 || index > 38) return null;

            return list[index];
        }

        private ZoneBehavior(int zoneGroup, string description, ZoneGroupDelay delay, bool levelOne, bool levelTwo, bool levelThree, bool reports, bool supervisory, bool restoral, ZoneAlarm alarm)
        {
            Group = (ZoneGroup)zoneGroup;
            Description = description;
            Delay = delay;
            ActiveWhenDisarmed = levelOne;
            ActiveWhenStay = levelTwo;
            ActiveWhenAway = levelThree;
            Reports = reports;
            Supervisory = supervisory;
            Restoral = restoral;
            Alarm = alarm;

        }

        /// <summary>
        /// Returns value indicating whether zone state triggers the assigned alarm
        /// </summary>
        /// <param name="level">Partition arming level</param>
        /// <returns></returns>
        public bool IsActive(ArmingLevel level)
        {
            switch (level)
            {
                case ArmingLevel.Away:
                    return ActiveWhenAway;
                case ArmingLevel.Stay:
                    return ActiveWhenStay;
                case ArmingLevel.Disarmed:
                    return ActiveWhenDisarmed;
                case ArmingLevel.PhoneTest:
                    return false;
                case ArmingLevel.SensorTest:
                    return false;
                case ArmingLevel.Indeterminate:
                    return false;
                default:
                    return false;
            }
        }
        public ZoneGroup Group { get; set; }
        public string Description { get; set; }
        public ZoneGroupDelay Delay { get; set; }
        public bool ActiveWhenDisarmed { get; set; }
        public bool ActiveWhenStay { get; set; }
        public bool ActiveWhenAway { get; set; }
        public bool Reports { get; set; }
        public bool Supervisory { get; set; }
        public bool Restoral { get; set; }
        public ZoneAlarm Alarm { get; set; }
    }

}
