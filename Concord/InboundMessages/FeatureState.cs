using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent when a change in feature state occurs and in response to a Dynamic Data Refresh Request.  
    /// </summary>
   
    public class FeatureState : Message
    {
        //Format: [LI] 22h 0Ch [PN] [AN] [FS1] [CS]
        public FeatureState(string message) : base(message)
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

        public Feature FeaturesOn
        {
            get
            {
                string token = this[4];
                return (Feature)ToInt(token);
            }
        }
    }



}
