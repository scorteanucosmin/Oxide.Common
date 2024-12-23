using System;

namespace Oxide.IO.Serialization
{
    /// <summary>
    /// Used to deserialize <typeparamref name="T0"/> from <typeparamref name="T1"/>
    /// </summary>
    /// <typeparam name="T0">The type deserializing to</typeparam>
    /// <typeparam name="T1">The serialized data</typeparam>
    public interface IDeserializer<T0, T1>
    {
        /// <summary>
        /// Deserializes the <typeparamref name="T1"/>
        /// </summary>
        /// <param name="inputBuffer">The data to deserialize</param>
        /// <param name="defaultInstance">A existing instance to use instead of creating a new one</param>
        /// <returns>The deserialized object</returns>
        T0 Deserialize(ReadOnlySpan<T1> inputBuffer, T0 defaultInstance = default);
    }
}
