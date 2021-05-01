using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent whenever an entry or exit delay is started or ended.
    /// </summary>
   
    public class EntryExitDelay : Message
    {
        //Format: 08h 22h 03h [PN] [AN] [DF] [DTh] [DTl] [CS]

        // DF:
        //    bit 5,4: 00 = standard,
        //             01 = extended,
        //             10 = twice extended
        //    bit 6: 1 = exit delay, 0 = entry delay
        //    bit 7: 1 = end delay,  0 = start delay

        public EntryExitDelay(string message) : base(message)
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

        public int DelayTimeSeconds
        {
            get
            {
                string token = string.Concat(this[5], this[6]);
                return ToInt(token);
            }
        }

        public DelayDuration Length
        {
            get
            {
                string token = this[4];
                int value = ToInt(token);

                int bitsFourAndFiveOrZero = value | 207;

                if (bitsFourAndFiveOrZero == 207)
                {
                    //11001111
                    return DelayDuration.Standard;
                }
                else if (bitsFourAndFiveOrZero == 223)
                {
                    //11011111
                    return DelayDuration.Extended;
                }
                else if (bitsFourAndFiveOrZero == 239)
                {
                    //11101111
                    return DelayDuration.TwiceExtended;
                }
                else
                {
                    throw new Exception("Delay length could not be parsed.");
                }
            }
        }

        public DelayPermission DelayPermission
        {
            get
            {
                string token = this[4];
                int value = ToInt(token);

                int bitSixOrZero = value | 191; //10111111

                if (bitSixOrZero == 191)
                {
                    //10111111
                    return DelayPermission.Entry;
                }
                else //else if (bitSixOrZero == 255)
                {
                    //11111111
                    return DelayPermission.Exit;
                }
            }
        }

        public DelayState DelayState
        {
            get
            {
                string token = this[4];
                int value = ToInt(token);

                int bitSixOrZero = value | 127; //01111111

                if (bitSixOrZero == 127)
                {
                    //01111111
                    return DelayState.Start;
                }
                else //else if (bitSixOrZero == 255)
                {
                    //11111111
                    return DelayState.End;
                }
            }
        }
    }



}

