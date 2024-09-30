extern alias References;
using References::Newtonsoft.Json;

namespace Oxide.IO.Serialization
{
    public class JsonSerializer : ISerializer
    {
        protected JsonSerializerSettings Settings { get; }

        public JsonSerializer() : this(JsonConvert.DefaultSettings())
        {
        }

        public JsonSerializer(JsonSerializerSettings settings)
        {
            Settings = settings ?? JsonConvert.DefaultSettings();
        }

        public T Deserialize<T>(string obj) => JsonConvert.DeserializeObject<T>(obj, Settings);

        public string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, Settings);
    }
}
