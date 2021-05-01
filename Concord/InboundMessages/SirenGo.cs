using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is used to begin all non-continuous cadences previously set up by the Siren Setup command.
    /// </summary>
   
    public class SirenGo : Message
    {
        //Format: 03h 22h 06h 2Bh
        public SirenGo(string message) : base(message)
        { }
    }
}
