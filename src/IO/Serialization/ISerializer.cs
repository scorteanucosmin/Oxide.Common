namespace Oxide.IO.Serialization
{
    /// <summary>
    /// Interface used to convert objects to and from string format
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the object to serialization string
        /// </summary>
        /// <typeparam name="T">The <see langword="type"/> to model the string from</typeparam>
        /// <param name="obj">The object instance</param>
        /// <returns>The serialized object</returns>
        string Serialize<T>(T obj);

        /// <summary>
        /// Deserializes a serialized string back into a given type
        /// </summary>
        /// <typeparam name="T">The <see langword="type"/> model to deseriaize to</typeparam>
        /// <param name="obj">The serialization <see langword="string"/></param>
        /// <returns>The <typeparamref name="T"/></returns>
        T Deserialize<T>(string obj);
    }
}
