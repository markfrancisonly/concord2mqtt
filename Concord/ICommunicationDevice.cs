//using System.ServiceModel;

namespace Automation.Concord
{
    /// <summary>
    /// Interface used to read and write data 
    /// </summary>
    public interface ICommunicationDevice
    {
        /// <summary>
        /// Synchronously reads one character from underlying communications link
        /// </summary>
        /// <returns></returns>
        int ReadChar();

        void WriteString(string data);

        /// <summary>
        /// Writes the specified string
        /// </summary>
        /// <param name="text"></param>
        void WriteChar(int data);

        /// <summary>
        /// Closes the port connection, sets the IsOpen property false
        /// </summary>
        void Close();

        /// <summary>
        /// Opens a new connection.
        /// </summary>
        void Open();

        int GetReconnectDelay();
    }
}
