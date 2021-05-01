using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is used to synchronize all continuous cadences previously set up by the Siren Setup command.
    /// </summary>
   
    public class SirenSynchronize : Message
    {
        //Format: 03h 22h 05h 2Ah
        public SirenSynchronize(string message) : base(message)
        { }
    }
}
