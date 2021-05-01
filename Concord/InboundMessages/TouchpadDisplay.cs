using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command sends the touchpad display text tokens to the Automation Module. 
    /// </summary>
   
    public class TouchpadDisplay : Message
    {
        //Format: [LI] 22h 09h [PN] [AN] [MT] [DT] [CS]
        public TouchpadDisplay(string message) : base(message)
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

        public DisplayTextType MessageType
        {
            get
            {
                string token = this[4];
                return (DisplayTextType)ToInt(token);
            }
        }

        public string Text
        {
            get
            {
                const int offset = 5;
                string asciiHexString = this.Data.Substring(offset * 2, this.Data.Length - (offset * 2));
                string text = DisplayTextCodeMap.GetText(asciiHexString);
                return text;
            }
        }

    }


}
