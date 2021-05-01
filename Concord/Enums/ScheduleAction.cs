using System;

namespace Automation.Concord.Panel
{
   
    public class Schedule
    {
        /// <summary>
        /// 0-15
        /// </summary>
        public int Id { get; set; }
        public ScheduleDays Days { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int StopHour { get; set; }
        public int StopMinute { get; set; }

        private int partition;
        public int Partition
        {
            get { return partition; }
        }

        public ScheduledAction Actions = ScheduledAction.None;

        public Schedule()
        { }

        public Schedule(Partition partition, int id)
        {
            this.partition = partition.Id;
            this.Id = id;
        }

        public static ScheduledAction MapScheduledEvent(ScheduledEvent messageScheduledEvent)
        {
            ScheduledAction action = ScheduledAction.None;
            switch (messageScheduledEvent)
            {
                case ScheduledEvent.Light1:
                    action = ScheduledAction.Light1;
                    break;
                case ScheduledEvent.Light2:
                    action = ScheduledAction.Light2;
                    break;
                case ScheduledEvent.Light3:
                    action = ScheduledAction.Light3;
                    break;
                case ScheduledEvent.Light4:
                    action = ScheduledAction.Light4;
                    break;
                case ScheduledEvent.Light5:
                    action = ScheduledAction.Light5;
                    break;
                case ScheduledEvent.Light6:
                    action = ScheduledAction.Light6;
                    break;
                case ScheduledEvent.Light7:
                    action = ScheduledAction.Light7;
                    break;
                case ScheduledEvent.Light8:
                    action = ScheduledAction.Light8;
                    break;
                case ScheduledEvent.Light9:
                    action = ScheduledAction.Light9;
                    break;
                case ScheduledEvent.Output1:
                    action = ScheduledAction.Output1;
                    break;
                case ScheduledEvent.Output2:
                    action = ScheduledAction.Output2;
                    break;
                case ScheduledEvent.Output3:
                    action = ScheduledAction.Output3;
                    break;
                case ScheduledEvent.Output4:
                    action = ScheduledAction.Output4;
                    break;
                case ScheduledEvent.Output5:
                    action = ScheduledAction.Output5;
                    break;
                case ScheduledEvent.Output6:
                    action = ScheduledAction.Output6;
                    break;
                case ScheduledEvent.LatchkeyOpen:
                    action = ScheduledAction.LatchkeyOpen;
                    break;
                case ScheduledEvent.LatchkeyClose:
                    action = ScheduledAction.LatchkeyClose;
                    break;
                case ScheduledEvent.ExceptionOpen:
                    action = ScheduledAction.ExceptionOpen;
                    break;
                case ScheduledEvent.ExceptionClose:
                    action = ScheduledAction.ExceptionClose;
                    break;
                case ScheduledEvent.AutoArmToLevel3:
                    action = ScheduledAction.AutoArmToLevel3;
                    break;

            }

            return action;
        }
    }

}