using System;

namespace Automation.Concord.InboundMessages
{
    /// <summary>
    /// The following commands are reserved for special use.
    /// </summary>
   
    public class Reserved : Message
    {
        public Reserved(string message)
            : base(message)
        { }
    }
}
