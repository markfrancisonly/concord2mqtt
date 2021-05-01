using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent for each zone programmed, in response to an equipment list request from the Automation Module.
    /// </summary>
   
    public class EquipmentListZone : Message
    {
        //Format: [LI] 03h [PN] [AN] [GN] [ZNh] [ZNl] [ZT] [ZS] [text] [CS]

        public EquipmentListZone(string message) : base(message)
        { }

        public int Partition
        {
            get
            {
                string token = this[1];
                return ToInt(token);
            }
        }

        public ZoneGroup ZoneGroup
        {
            get
            {
                string token = this[3];
                return (ZoneGroup)ToInt(token);
            }
        }

        public int Zone
        {
            get
            {
                string token = this[5];
                return ToInt(token);
            }
        }

        public ZoneType ZoneType
        {
            get
            {
                string token = this[6];
                return (ZoneType)ToInt(token);
            }
        }

        public ZoneState ZoneState
        {
            get
            {
                string token = this[7];
                return (ZoneState)ToInt(token);
            }
        }

        public string Text
        {
            get
            {
                const int offset = 8;
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
