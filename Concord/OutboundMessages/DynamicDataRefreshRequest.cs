using System;

namespace Automation.Concord.OutboundMessages
{

    /// <summary>
    /// Automation Module requests data refreshing with this command.   Panel sends Arming Level, 
    /// non-normal Zone Status, Alarm/Trouble Status commands in response.  Concord will 
    /// also respond with the Panel Type (01h), Feature State (22h/0Ch), Temperature (22h/0Dh), 
    /// Light State (23h/01h), and Time and Date (22h/0Eh) commands.  
    /// </summary>
   
    public class DynamicDataRefreshRequest : Message
    {
        private const string MESSAGE = "022022";

        //Format: 02h 20h 22h
        public DynamicDataRefreshRequest() : this(MESSAGE) { }
        public DynamicDataRefreshRequest(string message) : base(message)
        { }
    }
}
