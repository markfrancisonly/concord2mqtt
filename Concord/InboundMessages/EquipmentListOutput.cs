using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent for each programmed output in response to an equipment list request.
    /// </summary>
   
    public class EquipmentListOutput : Message
    {
        //Format: [LI] 07h [ONh] [ONl] [OS] [ID1] [ID2] [ID3] [ID4] [ID5] [text] [CS]
        public EquipmentListOutput(string message) : base(message)
        { }

        /// <summary>
        /// 1-70
        /// </summary>
        public int Output
        {
            get
            {
                string token = this[2];
                return ToInt(token);
            }
        }

        public OutputState OutputState
        {
            get
            {
                string token = this[3];
                int state = ToInt(token);

                //Bit 0 = on(1),off(0)
                //Bit 1 = pulse (1)

                if (state > 2)
                {
                    return OutputState.PulseOn;
                }
                else if (state > 1)
                {
                    // pulse off?
                    return OutputState.PulseOff;
                }
                else if (state == 1)
                {
                    return OutputState.On;
                }
                else if (state == 0)
                {
                    return OutputState.Off;
                }
                throw new Exception("Could not parse output state");
            }
        }

        /// <summary>
        ///     
        ///Superbus output	ID1-ID3 = device ID
        ///    ID4 = output page
        ///    ID5 = output bit mask
        ///
        ///SnapCard output	ID1-ID2 = 0
        ///    ID3-ID4 = 1
        ///    ID5 = output bit mask
        ///
        ///Onboard output	ID1-ID2 = 0
        ///    ID3 = 1
        ///    ID4 = 2
        ///    ID5 = output bit mask
        /// </summary>
        public string Id
        {
            get
            {
                string token = string.Concat(this[4], this[5], this[6], this[7], this[8]);
                return token;
            }
        }

        public string Text
        {
            get
            {
                const int offset = 9;
                int messageLength = ToInt(this.LastIndex);
                if (messageLength > offset + 1)
                {
                    string asciiHexString = this.Data.Substring(offset * 2, this.Data.Length - (offset * 2));
                    string text = DisplayTextCodeMap.GetText(asciiHexString);
                    return text;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

    }


}
