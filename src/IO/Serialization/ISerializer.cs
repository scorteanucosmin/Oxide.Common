namespace Oxide.IO.Serialization
{
    /// <summary>
    /// Used to convert <typeparamref name="T0"/> and <typeparamref name="T1"/>
    /// </summary>
    /// <typeparam name="T0">The type converting from</typeparam>
    /// <typeparam name="T1">The type converting to</typeparam>
    public interface ISerializer<T0, T1>
    {
        /// <summary>
        /// Serializes the <typeparamref name="T0"/>
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>The serialized object</returns>
        T1 Serialize(T0 obj);

        /// <summary>
        /// Deserializes the <typeparamref name="T1"/>
        /// </summary>
        /// <param name="data">The data to deserialize</param>
        /// <returns>The deserialized object</returns>
        T0 Deserialize(T1 data);
    }
}
