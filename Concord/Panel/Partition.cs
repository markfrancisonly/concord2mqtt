using System.Collections.Generic;

namespace Automation.Concord.Panel
{
    public class Partition
    {
        public const int CAPABILITY_COUNT_LIGHTS = 9;
        public const int CAPABILITY_COUNT_SCHEDULES = 16;
        public readonly Panel Panel;
 
        public Partition(Panel panel, int id)
        {
            this.Panel = panel;
            this.Id = id;
            this.LastArming = new LastArming();

            Lights = new Dictionary<int, Light>(CAPABILITY_COUNT_LIGHTS);
            for (int i = 0; i < CAPABILITY_COUNT_LIGHTS; i++)
            {
                Light item = new Light(this, i + 1);
                Lights.Add(item.Id, item);
            }

            Schedules = new Dictionary<int, Schedule>(CAPABILITY_COUNT_SCHEDULES);
            for (int i = 0; i < CAPABILITY_COUNT_SCHEDULES; i++)
            {
                Schedule item = new Schedule(this, i);
                Schedules.Add(item.Id, item);
            }
        }

        public AlarmType? AlarmType
        {
            get;
            private set;
        }

        public AlertClass? Alert
        {
            get;
            private set;
        }

        public bool? AllLights
        {
            get;
            set;
        }

        public ArmingLevel? ArmingLevel
        {
            get;
            private set;
        }

        public bool? ArmingProtest
        {
            get;
            set;
        }
     
        public bool? Chime
        {
            get;
            set;
        }

        public string DisplayText
        {
            get;
            set;
        }

        public bool? EnergySaver
        {
            get;
            set;
        }

        /// <summary>
        /// 1-6
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        private bool? inAlarm;
        public bool? InAlarm
        {
            get
            {
                if (inAlarm == null)
                {
                    // on connection established, alarm is not known? can we get this from siren state?
                    List<Zone> alarmedZones = Panel.GetZones(this.Id, ZoneState.Alarm);
                    if (alarmedZones.Count != 0)
                    {
                        inAlarm = true;
                    }
                    return inAlarm;
                }
                return inAlarm;
            }
            private set { inAlarm = value; }
        }

        public bool? IsAlarmPending
        {
            get;
            set;
        }

        public bool? IsArmingPending
        {
            get;
            set;
        }

 
        public LastArming LastArming
        {
            get;
            set;
        }

        public bool? Latchkey
        {
            get;
            set;
        }

        public Dictionary<int, Light> Lights
        {
            get;
            set;
        }

        /// <summary>
        /// Must be loaded from configuration settings since partition naming is not implemented on Concord 4
        /// </summary>
        public string Name { get; set; }

        public bool? NoDelay
        {
            get;
            set;
        }

        public bool? QuickArm
        {
            get;
            set;
        }

        public Dictionary<int, Schedule> Schedules
        {
            get;
            set;
        }

        public bool? SilentArming
        {
            get;
            set;
        }

        /// <summary>
        /// Set partition alarm state
        /// </summary>
        /// <param name="alert"></param>
        /// <param name="type"></param>
        /// <returns>true if changed, or null if unknown</returns>
        public bool? SetAlarm(AlertClass alert, AlarmType type)
        {
            bool? changed = null;
            if (this.Alert != alert)
            {
                if (this.Alert != null)
                    changed = true;

                this.Alert = alert;
            }
            else
            {
                changed = false;
            }
            if (this.AlarmType != type)
            {
                if (this.AlarmType != null)
                    changed = true;

                this.AlarmType = type;
                changed = true;
            }
            else
            {
                changed = false;
            }
            if (this.Alert == AlertClass.Alarm || this.Alert == AlertClass.AlarmRestoral)
            {
                switch (type)
                {
                    case Concord.AlarmType.Fire:
                    case Concord.AlarmType.FirePanic:
                    case Concord.AlarmType.Police:
                    case Concord.AlarmType.PolicePanic:
                    case Concord.AlarmType.Auxiliary:
                    case Concord.AlarmType.AuxiliaryPanic:
                    case Concord.AlarmType.KeystrokeViolation:
                    case Concord.AlarmType.Duress:
                    case Concord.AlarmType.ExitFault:
                    case Concord.AlarmType.CarbonMonoxide:
                    case Concord.AlarmType.EntryExit:
                    case Concord.AlarmType.Perimeter:
                    case Concord.AlarmType.Interior:
                        InAlarm = true;
                        break;
                }
            }
            return changed;
        }

        /// <summary>
        /// Initialize arming without knowing alarm state
        /// </summary>
        /// <param name="newArmingLevel"></param>
        /// <returns>true if there was a change, or null if not known</returns>
        public bool? InitializeArmingLevel(ArmingLevel newArmingLevel)
        {
            ArmingLevel? oldArmingLevel = ArmingLevel;
            ArmingLevel = newArmingLevel;

            // last arming user isn't known at initialization
            if (newArmingLevel == Concord.ArmingLevel.Disarmed)
            {
                Disarm(oldArmingLevel, newArmingLevel);
            }

            if (oldArmingLevel == null)
                return null;
            else
                return (oldArmingLevel != newArmingLevel);
        }

        /// <summary>
        /// Set arming level based on arming event
        /// </summary>
        /// <param name="newArmingLevel"></param>
        /// <param name="username"></param>
        /// <param name="userid"></param>
        /// <param name="userclass"></param>
        /// <param name="keyfob"></param>
        /// <param name="autoArmed"></param>
        /// <returns>true if there was a change, or null if not known</returns>
        public bool? SetArmingLevel(ArmingLevel newArmingLevel, string username, int? userid, UserClass? userclass, bool? keyfob, bool autoArmed)
        {
            ArmingLevel? oldArmingLevel = ArmingLevel;
            ArmingLevel = newArmingLevel;

            LastArming = new LastArming(newArmingLevel, username, userid, userclass, keyfob, autoArmed);

            if (newArmingLevel == Concord.ArmingLevel.Disarmed)
            {
                Disarm(oldArmingLevel, newArmingLevel);
            }

            if (oldArmingLevel == null)
                return null;
            else 
                return (oldArmingLevel != newArmingLevel);
        }

        private void Disarm(ArmingLevel? oldArmingLevel, ArmingLevel? newArmingLevel)
        {
            if (newArmingLevel == Concord.ArmingLevel.Disarmed && oldArmingLevel != null)
            {
                // changed to disarm from being armed, any alarms are disabled
                IsAlarmPending = false;
                IsArmingPending = false;
                InAlarm = false;
            }
        }
    }
}