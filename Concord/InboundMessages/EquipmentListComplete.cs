using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent after all the 03h and 05h commands have been sent in response to an equipment list request.  
    /// </summary>
   
    public class EquipmentListComplete : Message
    {
        //Format: 02h 08h 0Ah
        public EquipmentListComplete(string message) : base(message)
        {
        }
    }
}
