using System;

namespace Automation.Concord.Panel
{
    public class Light
    {
        public Light() { }

        internal Light(Partition partition, int id)
        {
            this.Partition = partition.Id;
            this.Id = id;
        }

        /// <summary>
        /// 1-9
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        public int Partition
        {
            get;
            set;
        }

        public bool? Enabled
        {
            get;
            set;
        }

        public void SetEnabled(bool enabled)
        {
            if (Enabled != enabled)
            {
                UpdateTime = DateTime.Now;
            }
            Enabled = enabled;
        }

        public DateTime? UpdateTime
        {
            get;
            set;
        }

        public override string ToString()
        {
            if (Enabled != null)
                return string.Format("Light {0} turned {1}.", Id, Enabled.Value ? "on" : "off");
            else
                return string.Format("Light {0} is indeterminate.", Id);
        }
    }
}
