namespace Oxide.IO.TransportMethods
{
    /// <summary>
    /// Writes a data to a underlying stream
    /// </summary>
    public interface ITransportTransmitter
    {
        /// <summary>
        /// Writes data from a buffer to a stream
        /// </summary>
        /// <param name="buffer">The buffer containing the data to write</param>
        /// <param name="index">The index to start reading from</param>
        /// <param name="count">The amount of bytes to write</param>
        void Write(byte[] buffer, int index, int count);
    }
}
