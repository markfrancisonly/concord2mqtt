using System;

namespace Automation.Concord
{
   
    public enum MessageResponse : int
    {
        Waiting = 0,
        /// <summary>
        /// Acknowledged
        /// </summary>
        ACK = 0x06,
        /// <summary>
        /// Negative acknowledgement
        /// </summary>
        NAK = 0x15,
        /// <summary>
        /// Not sent or failed repeatedly
        /// </summary>
        Expired,
        /// <summary>
        /// Timed out
        /// </summary>
        Timeout,
        /// <summary>
        /// Message already waiting
        /// </summary>
        Duplicate
    }
}
