using System;

namespace Automation.Concord.OutboundMessages
{
    /// <summary>
    /// Automation Module requests equipment list with this command.  Panel sends the following ‘Send Equipment List’ commands in response to this command.  In addition, Advent will send the Panel Type command (01h).
    /// </summary>
   
    public class FullEquipmentListRequest : Message
    {
        //Format: 02h 02h 04h
        private const string MESSAGE = "020204";

        public FullEquipmentListRequest() : base(MESSAGE) { }
        public FullEquipmentListRequest(string message)
            : base(message)
        { }
    }
}
