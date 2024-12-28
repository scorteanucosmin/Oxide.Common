using System;

namespace Oxide.IO.Serialization
{
    /// <summary>
    /// Provides methods for reading data from a buffer into an instance of a type.
    /// </summary>
    /// <typeparam name="TInstance">The type of the instance to be populated with data from the buffer.</typeparam>
    /// <typeparam name="TBufferType">The type of the elements in the buffer.</typeparam>
    public interface ITypeReader<TInstance, TBufferType>
    {
        /// <summary>
        /// Determines whether the specified buffer contains sufficient data to populate an instance.
        /// </summary>
        /// <param name="buffer">The buffer containing the data, represented as a read-only span.</param>
        /// <returns>
        /// <c>true</c> if the buffer contains enough data to populate the instance; otherwise, <c>false</c>.
        /// </returns>
        bool CanRead(ReadOnlySpan<TBufferType> buffer);

        /// <summary>
        /// Reads data from the specified buffer and populates the given instance.
        /// </summary>
        /// <param name="buffer">The buffer containing the data, represented as a read-only span.</param>
        /// <param name="instance">
        /// A reference to the instance that will be populated with data from the buffer.
        /// The instance is modified in place.
        /// </param>
        /// <returns>
        /// The number of elements read from the buffer.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the buffer is too small to read the data for the instance.</exception>
        int Read(ReadOnlySpan<TBufferType> buffer, ref TInstance instance);
    }
}
