namespace Oxide.IO.Serialization.Binary
{
    /// <summary>
    /// Defines methods for serializing an instance of a type into binary data and writing it to a buffer.
    /// </summary>
    /// <typeparam name="TInstance">The type of the instance to be serialized into binary form.</typeparam>
    public interface IBinaryWriter<TInstance> : ITypeWriter<TInstance, byte>
    {
    }
}
