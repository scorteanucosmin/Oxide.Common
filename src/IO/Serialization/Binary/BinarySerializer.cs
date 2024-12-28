using System;
using System.Collections.Generic;
using System.Text;

namespace Oxide.IO.Serialization.Binary
{
    public class BinarySerializer
    {
        private static Dictionary<Type, object> BinaryReaders { get; }

        private static Dictionary<Type, object> BinaryWriters { get; }

        static BinarySerializer()
        {
            RegisterBlittableType<byte>();
            RegisterBlittableType<sbyte>();
            RegisterBlittableType<short>();
            RegisterBlittableType<ushort>();
            RegisterBlittableType<int>();
            RegisterBlittableType<uint>();
            RegisterBlittableType<long>();
            RegisterBlittableType<ulong>();
            RegisterBlittableType<char>();
            RegisterBlittableType<float>();
            RegisterBlittableType<double>();
            RegisterBlittableType<decimal>();
            RegisterBlittableType<bool>();
            StringTypeSerializer strings = new StringTypeSerializer(Encoding.UTF8);
            BinaryReaders[typeof(string)] = strings;
            BinaryWriters[typeof(string)] = strings;
            PrimitiveTypeSerializer<long> longs = (PrimitiveTypeSerializer<long>)BinaryReaders[typeof(long)];
            DateTimeTypeSerializer dtSerializer = new DateTimeTypeSerializer(longs);
            BinaryReaders[typeof(DateTime)] = dtSerializer;
            BinaryWriters[typeof(DateTime)] = dtSerializer;
        }

        public static void RegisterBlittableType<T>() where T : struct
        {
            Type type = typeof(T);
            object rw = new PrimitiveTypeSerializer<T>();

            lock (BinaryReaders)
            {
                BinaryReaders[type] = rw;
            }

            lock (BinaryWriters)
            {
                BinaryWriters[type] = rw;
            }
        }
    }
}
