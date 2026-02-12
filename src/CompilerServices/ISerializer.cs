using System.IO;

namespace Oxide.CompilerServices;

public interface ISerializer
{
    public void SerializeToStream<T>(MemoryStream memoryStream, T type, int bufferSize) where T : class;

    public byte[] Serialize<T>(T type) where T : class;

    public T Deserialize<T>(byte[] data) where T : class;
}
