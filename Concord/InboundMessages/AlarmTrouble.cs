using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent to identify alarm and trouble conditions as well as several other system events.  Events are specified by three numbers; General Type, Specific Type, and Event Specific Data.  The lists below show all the events, categorized by General Type.
    /// </summary>
   
    public class AlarmTrouble : Message
    {
        //Format: 0Dh 22h 02h [PN] [AN] [ST] [SNh] [SNm] [SNl] [GT] [ST] [ESh] [ESl] [CS]
        public AlarmTrouble(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Partition 1-6
        /// </summary>
        public int Partition
        {
            get
            {
                string token = this[2];
                return ToInt(token);
            }

        }

        /// <summary>
        /// Source device Unit ID, or null if source is a zone
        /// </summary>
        public int? SourceDeviceUnitId
        {
            get
            {
                if (Source == SourceDeviceType.Zone) return null;

                string token = string.Concat(this[5], this[6], this[7]);
                return ToInt(token);
            }

        }

        /// <summary>
        /// Zone 1-96, 0 if source is not a zone
        /// </summary>
        public int Zone
        {
            get
            {
                if (Source != SourceDeviceType.Zone) return 0;

                string token = this[7];
                return ToInt(token);
            }
        }

        public SourceDeviceType Source
        {
            get
            {
                string token = this[4];
                return (SourceDeviceType)ToInt(token);
            }
        }

        public AlertClass GeneralEventType
        {
            get
            {
                string token = this[8];
                return (AlertClass)ToInt(token);
            }
        }

        public int SpecificEventCode
        {
            get
            {
                string token = this[9];
                return ToInt(token);
            }
        }

        public int SpecificEventData
        {
            get
            {
                string token = string.Concat(this[10], this[11]);
                return ToInt(token);
            }

        }
    }


}
