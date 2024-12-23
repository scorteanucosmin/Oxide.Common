using System;

namespace Oxide.IO.Serialization
{
    /// <summary>
    /// Used to serialize <typeparamref name="T1"/> to <typeparamref name="T0"/>
    /// </summary>
    /// <typeparam name="T0">The type serializing to</typeparam>
    /// <typeparam name="T1">The type to serialize</typeparam>
    public interface ISerializer<T0, T1>
    {
        /// <summary>
        /// Serializes <typeparamref name="T1"/> to <typeparamref name="T0"/>
        /// </summary>
        /// <param name="obj">The <typeparamref name="T1"/> to serialize</param>
        /// <param name="outputBuffer">The span of the original buffer to write to</param>
        /// <returns>The amount of bytes serialized into the buffer</returns>
        int Serialize(T0 obj, Span<T1> outputBuffer);
    }
}
