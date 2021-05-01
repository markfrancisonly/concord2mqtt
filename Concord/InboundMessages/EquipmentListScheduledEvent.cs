using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent for each scheduled event in response to an equipment list request.  
    /// </summary>
   
    public class EquipmentListScheduledEvent : Message
    {
        //Format: [LI] 0Bh [PN] [AN] [ET] [S1] [S2] [S3] [S4] [CS]
        public EquipmentListScheduledEvent(string message) : base(message)
        { }


        /// <summary>
        /// Partition 1-6
        /// </summary>
        public int Partition
        {
            get
            {
                string token = this[1];
                return ToInt(token);
            }
        }

        public ScheduledEvent ScheduledEvent
        {
            get
            {
                string token = this[3];
                return (ScheduledEvent)ToInt(token);
            }
        }

        /// <summary>
        /// 16 dimension array containing true/false to indicate if event is assigned to schedule at respective index
        /// </summary>
        public bool[] ScheduleAssignment
        {
            get
            {
                bool[] result = new bool[16];

                int hbyte = ToInt(this[4]);
                int lbyte = ToInt(this[5]);

                for (int i = 0; i < 8; i++)
                {
                    int bit = (int)Math.Pow(2, i);
                    result[i] = (hbyte & bit) == bit;
                }
                for (int i = 8; i < 16; i++)
                {
                    int bit = (int)Math.Pow(2, i - 8);
                    result[i] = (lbyte & bit) == bit;
                }

                return result;
            }
        }
    }



}
