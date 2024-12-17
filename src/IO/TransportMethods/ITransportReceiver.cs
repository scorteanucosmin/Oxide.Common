using System;

namespace Oxide.IO.TransportMethods
{
    /// <summary>
    /// Reads messages from a underlying stream
    /// </summary>
    public interface ITransportReceiver
    {
        /// <summary>
        /// Reads data from a stream
        /// </summary>
        /// <param name="buffer">The buffer to read data into</param>
        /// <param name="index">The position in the buffer to start reading</param>
        /// <param name="count">The amount of bytes to read</param>
        /// <returns>The amound of bytes read</returns>
        int Read(Span<byte> buffer, int index, int count);
    }
}
