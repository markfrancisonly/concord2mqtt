using System;
using System.Threading;

namespace Automation.Concord.Panel
{

    public class Zone : IStateChangeWaitHandle
    {

        public Zone() { }

        public Zone(int partition, int id)
        {
            this.Partition = partition;
            this.Id = id;
            this.Text = "Zone " + id.ToString();
            this.State = null;
        }

        private AutoResetEvent stateChangeSignal = new AutoResetEvent(false);

        /// <summary>
        /// 1-96
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Zone state. Any change to this value will cause the UpdateTime and PreviousState properties to be updated accordingly.
        /// </summary>
        public ZoneState? State
        {
            get;
            set;
        }

        /// <summary>
        /// Flips state change waithandle signal if there is a change
        /// </summary>
        /// <param name="newZoneState"></param>
        /// <returns>true if there was a change</returns>
        public bool? SetState(ZoneState newZoneState)
        {
            
            if (State == null)
            {
                if (newZoneState != State)
                {
                    State = newZoneState;
                    stateChangeSignal.Set();
                }
                return null;
            }
            else if (newZoneState != State)
            {
                State = newZoneState;
                stateChangeSignal.Set();
                return true;
            }
            else
            {
                return false;
            }
        }

        WaitHandle IStateChangeWaitHandle.StateChange
        {
            get
            {
                return stateChangeSignal;
            }
        }

        public ZoneType? Type
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }

        public ZoneGroup? Group
        {
            get;
            set;
        }

        public ZoneBehavior? Behavior
        {
            get
            {
                return ZoneBehavior.GetZoneBehavior(this);
            }
        }

        public int Partition
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("Zone {0}, '{1}' {2}", Id, Text, State != null ? State.ToString().ToLower() : "indeterminate");
        }

        public bool? IsSpecialized
        {
            get
            {

                if (Group == null) return null;

                switch (Group)
                {
                    case ZoneGroup.Auxiliary:
                    case ZoneGroup.Auxiliary_SirenConfirmedReport:
                    case ZoneGroup.AuxiliaryUnsupervised:
                    case ZoneGroup.AuxiliaryUnsupervised_SirenConfirmedReport:
                    case ZoneGroup.CarbonMonoxide:
                    case ZoneGroup.Fire:
                    case ZoneGroup.FreezeSensor:
                    case ZoneGroup.LocalAuxiliary:
                    case ZoneGroup.LocalAuxiliarySirenConfirmedRestoral:
                    case ZoneGroup.LocalChime:
                    case ZoneGroup.LocalPolice:
                    case ZoneGroup.LocalPoliceDelayed:
                    case ZoneGroup.LocalPolice_ReportWhenAway:
                    case ZoneGroup.OutputModule:
                    case ZoneGroup.OutputModuleLatched:
                    case ZoneGroup.OutputModuleLatchedUnsupervised:
                    case ZoneGroup.Police:
                    case ZoneGroup.PoliceSilent:
                    case ZoneGroup.PoliceSilentUnsupervised:
                    case ZoneGroup.PoliceUnsupervised:
                    case ZoneGroup.SirenSupervised:
                    case ZoneGroup.SpecialIntrusion:
                    case ZoneGroup.SpecialIntrusionDelayed:
                    case ZoneGroup.WaterSensor:
                        return true;
                }
                return false;
            }
        }

        public bool? IsPerimeter
        {
            get
            {
                if (this.Group == null)
                    return null;

                else if (this.Group == ZoneGroup.Perimeter ||
                    this.Group == ZoneGroup.PerimeterDelayed ||
                    this.Group == ZoneGroup.PerimeterExtendedDelay ||
                    this.Group == ZoneGroup.PerimeterTwiceExtendedDelay)
                    return true;
                else
                    return false;
            }
        }

        public bool? IsMotion
        {
            get
            {
                if (this.Group == null)
                    return null;

                else if (this.Group == ZoneGroup.InteriorMotion ||
                    this.Group == ZoneGroup.InteriorMotion_StayOrAway ||
                    this.Group == ZoneGroup.InteriorMotionCrosszone ||
                    this.Group == ZoneGroup.InteriorMotionDelayed)
                    return true;
                else
                    return false;
            }
        }

        public bool? IsInterior
        {
            get
            {
                if (this.Group == null)
                    return null;

                else if (this.Group == ZoneGroup.InteriorDoor ||
                    this.Group == ZoneGroup.InteriorDoor_StayOrAway ||
                    this.Group == ZoneGroup.InteriorDoorDelayed ||
                    IsMotion == true)
                    return true;
                else
                    return false;
            }
        }

    }
}
