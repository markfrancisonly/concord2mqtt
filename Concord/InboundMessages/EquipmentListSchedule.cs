using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent for each schedule in response to an equipment list request. 
    /// </summary>
   
    public class EquipmentListSchedule : Message
    {
        //Format: [LI] 0Ah [PA] [AA] [SN] [SH] [SM] [PH] [PM] [DY] [SV] [CS]
        public EquipmentListSchedule(string message) : base(message)
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
        /// <summary>
        /// 0-15
        /// </summary>
        public int Schedule
        {
            get
            {
                string token = this[3];
                return ToInt(token);
            }
        }

        public int StartHour
        {
            get
            {
                string token = this[4];
                return ToInt(token);
            }
        }

        public int StartMinute
        {
            get
            {
                string token = this[5];
                return ToInt(token);
            }
        }

        public int StopHour
        {
            get
            {
                string token = this[6];
                return ToInt(token);
            }
        }

        public int StopMinute
        {
            get
            {
                string token = this[7];
                return ToInt(token);
            }
        }

        public ScheduleDays Days
        {
            get
            {
                string token = this[8];
                return (ScheduleDays)ToInt(token);
            }
        }

    }


}
