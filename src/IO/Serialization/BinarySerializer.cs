using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Oxide.IO.Serialization
{
    public sealed class BinarySerializer<T> : ISerializer<byte[], T>, IDeserializer<T, byte[]>, IDisposable where T : class
    {
        private static Dictionary<MemberInfo, TypeConverter> ConverterCache { get; }

        private static List<MemberInfo> SerializableMembers { get; }


        private bool disposedValue;

        private MemoryStream MemoryStream { get; }

        private byte[] Buffer { get; }

        #region Constructor & Dispose

        static BinarySerializer()
        {
            SerializableMembers = new List<MemberInfo>();
            ConverterCache = new Dictionary<MemberInfo, TypeConverter>();

            IEnumerable<MemberInfo> members = typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => Attribute.IsDefined(m, typeof(IgnoreDataMemberAttribute)));
            SerializableMembers.AddRange(members);
        }

        public BinarySerializer(int bufferSize = 4096)
        {
            Buffer = new byte[bufferSize];
            MemoryStream = new MemoryStream(Buffer);
            BinaryWriter = new BinaryWriter(MemoryStream, Encoding.UTF8, true);
            BinaryReader = new BinaryReader(MemoryStream, Encoding.UTF8, true);
        }

        ~BinarySerializer()
        {
            Dispose(disposing: false);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    BinaryReader.Dispose();
                    BinaryWriter.Dispose();
                    MemoryStream.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Serializer

        private BinaryWriter BinaryWriter { get; }

        public void Serialize(T obj, Span<byte> outputBuffer)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            lock (MemoryStream)
            {
                try
                {
                    SerializeObject(BinaryWriter, obj);
                    BinaryWriter.Flush();
                    MemoryStream.GetBuffer().AsSpan(0, (int)MemoryStream.Length).CopyTo(outputBuffer);
                }
                finally
                {
                    MemoryStream.SetLength(0);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SerializeObject(BinaryWriter writer, T obj)
        {
            foreach (MemberInfo member in SerializableMembers)
            {
                Type memberType = GetMemberType(member);
                object val = GetValue(member, obj);
                TypeConverter converter = GetCachedConverter(member, memberType);
                if (converter.CanConvertTo(typeof(string)))
                {
                    if (val == null)
                    {
                        writer.Write(string.Empty);
                    }
                    else
                    {
                        string serializedValue = converter.ConvertToInvariantString(val);
                        writer.Write(serializedValue ?? string.Empty);
                    }
                }
                else
                {
                    throw new NotSupportedException($"Member type {memberType} does not have a TypeConverter that converts to string");
                }
            }
        }

        #endregion

        #region Deserializer

        private BinaryReader BinaryReader { get; }

        public T Deserialize(ReadOnlySpan<byte> inputBuffer)
        {
            lock (MemoryStream)
            {
                try
                {
                    MemoryStream.Write(inputBuffer);
                    MemoryStream.Position = 0;
                    return DeserializeObject(BinaryReader);
                }
                finally
                {
                    MemoryStream.SetLength(0);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T DeserializeObject(BinaryReader reader)
        {
            T instance = Activator.CreateInstance<T>();

            foreach (MemberInfo member in SerializableMembers)
            {
                Type memberType = GetMemberType(member);
                TypeConverter converter = TypeDescriptor.GetConverter(memberType);
                string serializedValue = BinaryReader.ReadString();
                object val = null;
                if (!string.IsNullOrEmpty(serializedValue))
                {
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        val = converter.ConvertFromInvariantString(serializedValue);
                    }
                    else
                    {
                        throw new NotSupportedException($"Member type {memberType} does not have a TypeConverter that converts from string");
                    }
                }
                SetValue(member, instance, val);
            }

            return instance;
        }

        #endregion

        #region Helpers

        private static object GetValue(MemberInfo member, T obj) => member switch
        {
            PropertyInfo prop => prop.GetValue(obj),
            FieldInfo field => field.GetValue(obj),
            _ => null
        };

        private static void SetValue(MemberInfo member, T obj, object value)
        {
            switch (member)
            {
                case PropertyInfo property when property.CanWrite:
                    property.SetValue(obj, value);
                    break;

                case PropertyInfo property when !property.CanWrite:
                    SetValue(GetBackingField(property), obj, value);
                    break;

                case FieldInfo field:
                    field.SetValue(obj, value);
                    break;
            }
        }

        private static Type GetMemberType(MemberInfo member)
        {
            Type type;
            switch (member)
            {
                case PropertyInfo property:
                    type = property.PropertyType;
                    break;

                case FieldInfo field:
                    type = field.FieldType;
                    break;

                default:
                    return null;
            }

            Type underlying = Nullable.GetUnderlyingType(type);

            return underlying ?? type;
        }

        private static TypeConverter GetCachedConverter(MemberInfo member, Type memberType)
        {
            if (!ConverterCache.TryGetValue(member, out TypeConverter converter))
            {
                converter = TypeDescriptor.GetConverter(memberType);
                ConverterCache[member] = converter;
            }

            return converter;
        }

        private static FieldInfo GetBackingField(PropertyInfo property)
        {
            string name = $"<{property.Name}>k__BackingField";
            return property.DeclaringType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        #endregion
    }
}
