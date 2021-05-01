using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent whenever a user lights control is made. 
    /// </summary>
   
    public class UserLights : Message
    {
        //Format: 0Bh 23h 02h [PN] [AN] [ST] [SNh] [SNm] [SNl] [LN] [LS] [CS]
        public UserLights(string message) : base(message)
        { }

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

        public SourceDeviceType Source
        {
            get
            {
                string token = this[4];
                return (SourceDeviceType)ToInt(token);
            }
        }

        /// <summary>
        /// Source device Unit ID, or null if source is a zone
        /// </summary>
        public string SourceDeviceUnitId
        {
            get
            {
                if (Source == SourceDeviceType.Zone) return null;

                string token = string.Concat(this[5], this[6], this[7]);
                return token;
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


        /// <summary>
        /// 0 = all lights, 1-32 specific light
        /// </summary>
        public int LightCode
        {
            get
            {
                string token = this[8];
                return ToInt(token);
            }
        }

        public bool Enabled
        {
            get
            {
                string token = this[9];
                return ToInt(token) == 1 ? true : false;
            }
        }
    }
}
