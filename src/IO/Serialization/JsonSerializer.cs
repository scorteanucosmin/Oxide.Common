extern alias References;

using Oxide.Pooling;
using References::Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Oxide.IO.Serialization
{
    public class JsonSerializer<T> : ISerializer<T, string>, ISerializer<T, byte[]>
    {
        protected JsonSerializerSettings Settings { get; }
        protected IPool<StringBuilder> StringPool { get; }
        protected Encoding FormatEncoding { get; }
        protected JsonSerializer Serializer { get; }

        public JsonSerializer(Encoding encoding = null, IPool<StringBuilder> stringPool = null) : this(JsonConvert.DefaultSettings(), encoding, stringPool)
        {   
        }

        public JsonSerializer(JsonSerializerSettings settings = null, Encoding encoding = null, IPool<StringBuilder> stringPool = null)
        {
            Settings = settings ?? JsonConvert.DefaultSettings();
            StringPool = stringPool ?? PoolFactory<StringBuilder>.NoPool;
            FormatEncoding = encoding ?? Encoding.UTF8;
            Serializer = JsonSerializer.CreateDefault(Settings);
        }

        public string Serialize(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            StringBuilder sb = StringPool.Take();

            try
            {
                using (StringWriter writer = new StringWriter(sb))
                {
                    Serializer.Serialize(writer, obj);
                    writer.Flush();
                }

                return sb.ToString();
            }
            finally
            {
                StringPool.Return(sb);
            }
        }

        public T Deserialize(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using StringReader reader = new StringReader(data);
            return (T)Serializer.Deserialize(reader, typeof(T));
        }

        byte[] ISerializer<T, byte[]>.Serialize(T obj)
        {
            string json = Serialize(obj);
            byte[] buffer = new byte[FormatEncoding.GetByteCount(json)];
            FormatEncoding.GetBytes(json, 0, json.Length, buffer, 0);
            return buffer;
        }

        public T Deserialize(byte[] data)
        {
            string json = FormatEncoding.GetString(data);
            return Deserialize(json);
        }
    }
}
