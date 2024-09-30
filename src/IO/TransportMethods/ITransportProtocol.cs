namespace Oxide.IO.TransportMethods
{
    /// <summary>
    /// A interface used to send and receive data
    /// </summary>
    public interface ITransportProtocol
    {
        /// <summary>
        /// Sends data
        /// </summary>
        /// <param name="data">The data to send</param>
        void Send(string data);

        /// <summary>
        /// Receives data
        /// </summary>
        /// <returns>The data received</returns>
        string Receive();
    }
}
