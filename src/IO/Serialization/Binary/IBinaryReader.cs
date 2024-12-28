namespace Oxide.IO.Serialization.Binary
{
    /// <summary>
    /// Defines methods for reading binary data from a buffer and deserializing it into an instance of a type.
    /// </summary>
    /// <typeparam name="TInstance">The type of the instance to be populated with data from the binary buffer.</typeparam>
    public interface IBinaryReader<TInstance> : ITypeReader<TInstance, byte>
    {
    }
}
