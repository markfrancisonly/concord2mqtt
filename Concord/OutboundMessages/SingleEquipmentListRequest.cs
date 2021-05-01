using System;

namespace Automation.Concord.OutboundMessages
{
    /// <summary>
    /// Automation Module requests a single equipment list parameter with this command.  Panel sends the requested ‘Send Equipment List’ command(s) in response to this command.
    /// </summary>
   
    public class SingleEquipmentListRequest : Message
    {
        //Format: 03h 02h [EP] [CS]
        private const string HEADER = "02";

        public SingleEquipmentListRequest(EquipmentListParameter equipment)
        {
            this.Data = HEADER + Message.ToAsciiHex((int)equipment);
        }
        public SingleEquipmentListRequest(string message) : base(message)
        { }
    }



}
