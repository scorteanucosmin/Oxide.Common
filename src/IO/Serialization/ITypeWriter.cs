using System;

namespace Oxide.IO.Serialization
{
    /// <summary>
    /// Provides methods for writing an instance of a type to a buffer.
    /// </summary>
    /// <typeparam name="TInstance">The type of the instance to be written.</typeparam>
    /// <typeparam name="TBufferType">The type of the elements in the buffer.</typeparam>
    public interface ITypeWriter<TInstance, TBufferType>
    {
        /// <summary>
        /// Determines whether the specified buffer has sufficient space to write the given instance.
        /// </summary>
        /// <param name="instance">The instance to be written.</param>
        /// <param name="buffer">The buffer to write to, represented as a read-only span.</param>
        /// <returns>
        /// <c>true</c> if the buffer has enough space to write the instance; otherwise, <c>false</c>.
        /// </returns>
        bool CanWrite(TInstance instance, ReadOnlySpan<TBufferType> buffer);

        /// <summary>
        /// Writes the specified instance to the provided buffer.
        /// </summary>
        /// <param name="instance">The instance to be written.</param>
        /// <param name="buffer">The buffer to write to, represented as a span.</param>
        /// <returns>
        /// The number of elements written to the buffer.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the buffer is too small to hold the serialized data.</exception>
        int Write(TInstance instance, Span<TBufferType> buffer);
    }
}
