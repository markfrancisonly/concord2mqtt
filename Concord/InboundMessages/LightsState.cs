using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent whenever a light state change occurs or in response to a Refresh command. 
    /// </summary>
   
    public class LightsState : Message
    {
        //[LI] 23h 01h [PN] [AN] [LS1] [LS2] [CS]

        public LightsState(string message) : base(message)
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

        public bool AllLights
        {
            get
            {
                //Bit 0 = All Lights
                //Bits 1-7 = Lights 1-7
                int hbyte = ToInt(this[4]);

                //Bit 0 = Light 8
                //Bit 1 = Light 9
                //int lbyte = ToInt(this[5]);

                if ((hbyte & 1) == 1)
                    return true; // all 
                else
                    return false;
            }
        }

        /// <summary>
        /// Array of length 10 representing each light's power on state. True is on, false is off.
        /// </summary>
        public bool[] LightState
        {
            get
            {
                bool[] result = new bool[] { true, true, true, true, true, true, true, true, true };

                //Bit 0 = All Lights
                //Bits 1-7 = Lights 1-7
                int hbyte = ToInt(this[4]);

                //Bit 0 = Light 8
                //Bit 1 = Light 9
                int lbyte = ToInt(this[5]);

                //if ((hbyte & 1) == 1)
                //{
                //    return result; // all 
                //}
                //else
                //{
                for (int i = 1; i < 8; i++)
                {
                    int bit = (int)Math.Pow(2, i);
                    result[i - 1] = (hbyte & bit) == bit;
                }
                for (int i = 8; i < 10; i++)
                {
                    int bit = (int)Math.Pow(2, i - 8);
                    result[i - 1] = (lbyte & bit) == bit;
                }

                return result;
                //}
            }
        }
    }
}
