extern alias References;

using System.IO;
using System.Text;
using References::Newtonsoft.Json;

namespace Oxide.CompilerServices;

public class Serializer : ISerializer
{
    private readonly JsonSerializer _jsonSerializer;

    public Serializer()
    {
        _jsonSerializer = new JsonSerializer();
    }

    public byte[] Serialize<T>(T type) where T : class
    {
        using MemoryStream memoryStream = new();
        using StreamWriter writer = new(memoryStream, Encoding.UTF8);
        _jsonSerializer.Serialize(writer, type);
        writer.Flush();
        return memoryStream.ToArray();
    }

    public T Deserialize<T>(byte[] data) where T : class
    {
        using MemoryStream memoryStream = new(data);
        using StreamReader reader = new(memoryStream, Encoding.UTF8);
        return (T)_jsonSerializer.Deserialize(reader, typeof(T));
    }
}
