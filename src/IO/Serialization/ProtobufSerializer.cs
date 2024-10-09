extern alias References;
using Oxide.Pooling;
using References::ProtoBuf;
using References::ProtoBuf.Meta;
using System;
using System.IO;
using System.Reflection;

namespace Oxide.IO.Serialization
{
    public class ProtobufSerializer<T> : ISerializer<T, byte[]>
    {
        protected RuntimeTypeModel Model { get; }

        private IPool<MemoryStream> MemoryPool { get; }

        public ProtobufSerializer(IPool<MemoryStream> memoryPool = null) : this(RuntimeTypeModel.Default, memoryPool)
        {
        }

        public ProtobufSerializer(RuntimeTypeModel model, IPool<MemoryStream> memoryPool = null)
        {
            MemoryPool = memoryPool ?? PoolFactory<MemoryStream>.NoPool;
            Model = model ?? throw new ArgumentNullException(nameof(model));
            RegisterLoop(model, typeof(T));
        }

        public T Deserialize(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            MemoryStream ms = MemoryPool.Take();
            try
            {
                ms.SetLength(0);
                ms.Write(data, 0, data.Length);
                ms.Position = 0;
                return Serializer.Deserialize<T>(ms);
            }
            finally
            {
                MemoryPool.Return(ms);
            }
        }

        public byte[] Serialize(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            MemoryStream ms = MemoryPool.Take();
            try
            {
                ms.SetLength(0);
                Serializer.Serialize(ms, obj);
                byte[] data = new byte[(int)ms.Length];
                Array.Copy(ms.GetBuffer(), 0, data, 0, ms.Length);
                return data;
            }
            finally
            {
                MemoryPool.Return(ms);
            }
        }

        private static void RegisterLoop(RuntimeTypeModel model, Type type)
        {
            if (type.IsPrimitive || type == typeof(string)|| model.IsDefined(type))
            {
                return;
            }

            var metaType = model.Add(type, false);

            int fieldNumber = 1;
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.IsSpecialName || !property.CanWrite || property.GetGetMethod().IsStatic)
                {
                    continue;
                }

                metaType.AddField(fieldNumber, property.Name);
                fieldNumber += 1;
                if (property.PropertyType.IsGenericType)
                {
                    foreach (Type generic in property.PropertyType.GetGenericArguments())
                    {
                        RegisterLoop(model, generic);
                    }
                }

                RegisterLoop(model, property.PropertyType);
            }
        }
    }
}
