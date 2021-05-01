using System;
using System.Collections.Generic;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent for each light in response to an equipment list request.
    /// </summary>
   
    public class EquipmentListLightToSensor : Message
    {
        //Format: [LI] 0Ch [PN] [AN] [L1] … [Ln] [CS]



        public EquipmentListLightToSensor(string message) : base(message)
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

        public List<int> AttachedLights
        {
            get
            {
                List<int> list = new List<int>();

                const int offset = 3;
                int wordCount = (this.Data.Length / 2) - offset;

                for (int i = 0; i < wordCount; i++)
                {
                    string token = this[offset + i];
                    list.Add(Message.ToInt(token));
                }

                //Ln = 01h-60h, 00h=none
                if (list.Count == 1 && list[0] == 0)
                    return new List<int>();
                else
                    return list;
            }
        }

    }
}
