using System;
using System.Collections.Generic;

namespace Automation.Concord.Panel
{
    public class Panel
    {
        public const int CAPABILITY_COUNT_PARTITIONS = 6;
        public const int CAPABILITY_COUNT_USERS = 255;//253;
        public const int CAPABILITY_COUNT_OUTPUTS = 70;
        public const int CAPABILITY_COUNT_ZONES = 96;

        private Dictionary<int, Partition> partitions;
        private Dictionary<int, Output> outputs;
        private Dictionary<int, User> users;
        private Dictionary<string, Device> devices;
        private Dictionary<int, Zone> zones;

        public string PanelTypeId
        {
            get; set;
        }

        public PanelType? PanelType
        {
            get
            {
                try
                {
                    if (PanelTypeId == null)return null;

                    return Enum.Parse<PanelType>(PanelTypeId);
                }
                catch { return null; }
            }
        }

        public string HardwareRevision
        {
            get; set;
        }

        public string SoftwareRevision
        {
            get; set;
        }

        public string SerialNumber
        {
            get; set;
        }

        /// <summary>
        /// Panel's internal time +30 or -30 seconds
        /// </summary>
        public DateTime DateTime
        {
            get; set;
        }

        public Panel()
        {
            Reset();
        }

        public void Reset()
        {
            Dictionary<int, Partition> resetPartitions = new Dictionary<int, Partition>(CAPABILITY_COUNT_PARTITIONS);
            for (int i = 1; i <= CAPABILITY_COUNT_PARTITIONS; i++)
            {
                Partition item = new Partition(this, i);
                if (partitions != null)
                {
                    // preserve concord data model extentions data
                    item.Name = partitions[i].Name ?? "Partition " + i.ToString();
                    item.LastArming = partitions[i].LastArming;
                }
                resetPartitions.Add(item.Id, item);
            }
            partitions = resetPartitions;

            devices = new Dictionary<string, Device>();

            zones = new Dictionary<int, Zone>(CAPABILITY_COUNT_ZONES);
            for (int i = 1; i <= CAPABILITY_COUNT_ZONES; i++)
            {
                // partition -1 for unassigned
                Zone item = new Zone(-1, i);
                zones.Add(item.Id, item);
            }

            outputs = new Dictionary<int, Output>(CAPABILITY_COUNT_OUTPUTS);
            for (int i = 1; i <= CAPABILITY_COUNT_OUTPUTS; i++)
            {
                Output item = new Output(i);
                outputs.Add(item.Id, item);
            }

            Dictionary<int, User> resetUsers = new Dictionary<int, User>(CAPABILITY_COUNT_USERS);
            for (int i = 0; i <= CAPABILITY_COUNT_USERS; i++)
            {
                User item = new User(i);
                if (users != null)
                {
                    // preserve configuration read concord data model extentions data
                    item.Name = users[i].Name ?? "User " + i.ToString();
                }
                resetUsers.Add(item.Id, item);
            }
            users = resetUsers;
        }

        /// <summary>
        /// Returns true any partition alarm has been tripped and is pending alarm state
        /// </summary>
        public bool? IsAlarmPending
        {
            get
            {
                foreach (Partition partition in Partitions.Values)
                {
                    if (partition.IsAlarmPending == null)
                        return null;

                    if (partition.IsAlarmPending.Value)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns true if any partition is in alarmed state
        /// </summary>
        public bool? InAlarm
        {
            get
            {
                foreach (Partition partition in Partitions.Values)
                {
                    if (partition.InAlarm == null)
                        return null;

                    if (partition.InAlarm.Value)
                        return true;
                }
                return false;
            }
        }

        public List<Zone> GetZones(int? partition)
        {
            var result = new List<Zone>();
            foreach (Zone zone in Zones.Values)
            {
                if (partition != null && zone.Partition != partition.Value) continue;
                result.Add(zone);
            }
            return result;
        }

        public List<Zone> GetZones(int? partition, ZoneState state)
        {
            var result = new List<Zone>();
            foreach (Zone zone in Zones.Values)
            {
                if (partition != null && zone.Partition != partition.Value) continue;
                if ((zone.State & state) == state)
                {
                    result.Add(zone);
                }
            }
            return result;
        }

        /// <summary>zones
        /// Returns list of zones matching specified state
        /// </summary>
        public List<Zone> GetZones(ZoneState state)
        {
            return GetZones(null, state);
        }

        /// <summary>
        /// When parition is null, all zones with a partition assignment will be returned
        /// </summary>
        /// <param name="partition">null for all partitions</param>
        /// <returns></returns>
        /// <summary>
        protected Dictionary<int, Zone> GetPartitionZones(int? partition)
        {
            Dictionary<int, Zone> result = new Dictionary<int, Zone>();
            foreach (Zone zone in Zones.Values)
            {
                // zone not assigned to a partition are inactive and will be ignored
                if (zone.Partition == -1) continue;

                if (partition != null && zone.Partition != partition.Value) continue;
                result.Add(zone.Id, zone);
            }
            return result;
        }

        /// <summary>
        /// Returns list of non-motion zones that are not closed, or open motion zones that are active in current arming level
        /// </summary>
        /// <param name="partition">null for all partitions</param>
        /// <returns></returns>
        public Dictionary<int, Zone> GetProblemZones(int? partition)
        {
            Dictionary<int, Zone> partitionZones = GetPartitionZones(partition);
            Dictionary<int, Zone> result = new Dictionary<int, Zone>();
            foreach (Zone zone in partitionZones.Values)
            {
                if (zone.State != ZoneState.Normal)
                {
                    if (zone.IsMotion == true && zone.State == ZoneState.Opened)
                    {
                        if (zone.Behavior != null)
                        {
                            if (Partitions[zone.Partition].ArmingLevel != null)
                            {
                                if (zone.Behavior.Value.IsActive(Partitions[zone.Partition].ArmingLevel.Value))
                                    // group behavior specifies that motion sensor is inactive
                                    continue;
                            }
                        }
                    }
                    result.Add(zone.Id, zone);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns list of non-motion zones that are not closed, including motion
        /// </summary>
        public Dictionary<int, Zone> GetOpenZones(int? partition)
        {
            Dictionary<int, Zone> partitionZones = GetPartitionZones(partition);
            Dictionary<int, Zone> result = new Dictionary<int, Zone>();
            foreach (Zone zone in partitionZones.Values)
            {
                if (zone.State != ZoneState.Normal)
                {
                    result.Add(zone.Id, zone);
                }
            }
            return result;
        }

        public Dictionary<int, Partition> Partitions => partitions;

        public Dictionary<int, Output> Outputs => outputs;

        public Dictionary<int, User> Users => users;

        public Dictionary<string, Device> Devices => devices;

        public Dictionary<int, Zone> Zones => zones;
    }
}