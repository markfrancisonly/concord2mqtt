using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// This command is sent if the panel’s automation buffer has overflowed resulting in the loss of system events.  This command should result in a Dynamic Data Refresh and Full Equipment List request from the Automation Device.
    /// </summary>
   
    public class AutomationEventLost : Message
    {
        public AutomationEventLost(string message) : base(message)
        { }
    }
}
