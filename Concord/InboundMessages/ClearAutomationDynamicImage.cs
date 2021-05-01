using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent on panel power up initialization and when a communication failure restoral 
    /// with the Automation Module occurs.  The Concord will also send this command when user or installer 
    /// programming mode is exited. 
    /// </summary>
   
    public class ClearAutomationDynamicImage : Message
    {
        public ClearAutomationDynamicImage(string message) : base(message) { }
    }
}
