extern alias References;
using References::Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace Oxide.IO.Serialization.Binary
{
    public sealed class BinarySerializer<T> : ISerializer<T, byte>, IDeserializer<T, byte> where T : class
    {
        private class MemberMap
        {
            public MemberInfo Member { get; }

            public Func<object, object> Getter { get; }

            public Action<object, object> Setter { get; }

            public TypeConverter Converter { get; }

            public Type MemberType { get; }

            public MemberMap(MemberInfo member, Type memberType, Func<object, object> getter, Action<object, object> setter, TypeConverter converter)
            {
                Member = member;
                MemberType = memberType;
                Getter = getter;
                Setter = setter;
                Converter = converter;
            }
        }

        private static List<MemberMap> MemberMaps { get; }

        private Encoding Encoder { get; }

        #region Constructor & Dispose

        static BinarySerializer()
        {
            MemberMaps = new List<MemberMap>();
            Compile();
        }

        public BinarySerializer(Encoding encoding = null)
        {
            Encoder = encoding ?? Encoding.UTF8;
        }

        #endregion

        #region Serializer


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Serialize(T obj, Span<byte> outputBuffer)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            int total = 0;
            foreach (var map in MemberMaps)
            {
                object val = GetValue(map, obj);
                int writeSize = sizeof(int);

                if (val != null)
                {
                    var type = val.GetType();

                    if (type.IsPrimitive || type.IsValueType)
                    {
                        int size = Marshal.SizeOf(type);
                        
                        if (size + sizeof(int) > outputBuffer.Length)
                        {
                            throw new ArgumentException($"Destination span is too small. Required: {sizeof(int) + size}, Available: {outputBuffer.Length}", nameof(outputBuffer));
                        }


                        GCHandle handle = GCHandle.Alloc(val, GCHandleType.Pinned);

                        try
                        {
                            IntPtr ptr = handle.AddrOfPinnedObject();
                            Span<byte> source;

                            unsafe
                            {
                                source = new Span<byte>(ptr.ToPointer(), size);
                            }

                            outputBuffer = outputBuffer.Write(size);
                            source.CopyTo(outputBuffer);
                            outputBuffer = outputBuffer[size..];
                            writeSize += size;
                        }
                        finally
                        {
                            handle.Free();
                        }
                    }
                    else if (map.MemberType == typeof(string))
                    {
                        var strValue = ((string)val).AsSpan();
                        int size = Encoder.GetByteCount(strValue);

                        if (sizeof(int) + size > outputBuffer.Length)
                        {
                            throw new ArgumentException($"Destination span is too small. Required: {sizeof(int) + size}, Available: {outputBuffer.Length}", nameof(outputBuffer));
                        }

                        outputBuffer = outputBuffer.Write(size);
                        outputBuffer = outputBuffer.Write(strValue, Encoder);
                        writeSize += size;
                    }
                    else if (map.Converter.CanConvertTo(typeof(byte[])))
                    {
                        byte[] byteVal = (byte[])map.Converter.ConvertTo(val, typeof(byte[]));

                        if (sizeof(int) + byteVal.Length > outputBuffer.Length)
                        {
                            throw new ArgumentException($"Destination span is too small. Required: {sizeof(int) + byteVal.Length}, Available: {outputBuffer.Length}", nameof(outputBuffer));
                        }

                        outputBuffer = outputBuffer.Write(byteVal.Length);
                        byteVal.CopyTo(outputBuffer);
                        outputBuffer = outputBuffer[byteVal.Length..];
                        writeSize += byteVal.Length;
                    }
                    else
                    {
                        throw new NotSupportedException($"Member {map.Member} is not supported. Either decorate it with IgnoreDataMember or create a TypeConverter to convert it to bytes");
                    }
                }
                else
                {
                    if (sizeof(int) > outputBuffer.Length)
                    {
                        throw new ArgumentException($"Destination span is too small. Required: {sizeof(int)}, Available: {outputBuffer.Length}", nameof(outputBuffer));
                    }

                    outputBuffer = outputBuffer.Write(0);
                }

                total += writeSize;
            }

            return total;
        }

        #endregion

        #region Deserializer


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize(ReadOnlySpan<byte> inputBuffer, T existingInstance = null)
        {
            existingInstance ??= Activator.CreateInstance<T>();

            foreach (var map in MemberMaps)
            {
                if (sizeof(int) > inputBuffer.Length)
                {
                    throw new ArgumentException($"Input buffer is too small to read length prefix. Required: {sizeof(int)} | Actual: {inputBuffer.Length}", nameof(inputBuffer));
                }

                int size = inputBuffer.Length;
                var workingSpan = inputBuffer[sizeof(int)..];

                if (size > workingSpan.Length)
                {
                    throw new ArgumentException($"Input buffer is too small to read {map.MemberType}. Required: {size} | Actual: {workingSpan.Length}", nameof(workingSpan));
                }

                workingSpan = workingSpan[..size];

                if (size > 0)
                {
                    if (map.MemberType.IsPrimitive || map.MemberType.IsValueType)
                    {
                        object value = Activator.CreateInstance(map.MemberType);
                        GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);

                        try
                        {
                            IntPtr ptr = handle.AddrOfPinnedObject();
                            unsafe
                            {
                                workingSpan.CopyTo(new Span<byte>(ptr.ToPointer(), size));
                            }
                            SetValue(map, existingInstance, value);
                            inputBuffer = inputBuffer[(sizeof(int) + size)..];
                        }
                        finally
                        {
                            handle.Free();
                        }
                    }
                    else if (map.MemberType == typeof(string))
                    {
                        string strValue = Encoder.GetString(workingSpan);

                        SetValue(map, existingInstance, strValue);
                        inputBuffer = inputBuffer[(sizeof(int) + size)..];
                    }
                    else if (map.Converter.CanConvertFrom(typeof(byte[])))
                    {
                        byte[] data = new byte[size];
                        workingSpan.CopyTo(data);
                        object convertedValue = map.Converter.ConvertFrom(data);
                        SetValue(map, existingInstance, convertedValue);
                        inputBuffer = inputBuffer[(sizeof(int) + size)..];
                    }
                    else
                    {
                        throw new NotSupportedException($"Member {map.Member} is not supported. Either decorate it with IgnoreDataMember or create a TypeConverter to convert it from bytes");
                    }
                }
                else
                {
                    object value = null;

                    if (map.MemberType.IsPrimitive || map.MemberType.IsValueType)
                    {
                        value = Activator.CreateInstance(map.MemberType);
                    }

                    SetValue(map, existingInstance, value);
                    inputBuffer = inputBuffer[sizeof(int)..];
                }
            }

            return existingInstance;
        }

        #endregion

        #region Helpers

        private static object GetValue(MemberMap map, T obj) => map.Getter(obj);

        private static void SetValue(MemberMap map, T obj, object value)
        {
            map.Setter(obj, value);
        }

        private static FieldInfo GetBackingField(PropertyInfo property)
        {
            string name = $"<{property.Name}>k__BackingField";
            return property.DeclaringType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static Func<object, object> CompileGetter(MemberInfo member)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var instanceCast = Expression.Convert(instance, member.DeclaringType);

            Expression memberAccess = member switch
            {
                PropertyInfo property => Expression.Property(instanceCast, property),
                FieldInfo field => Expression.Field(instanceCast, field),
                _ => throw new NotSupportedException($"Unsupported member type: {member.MemberType}")
            };

            var convert = Expression.Convert(memberAccess, typeof(object));
            return Expression.Lambda<Func<object, object>>(convert, instance).Compile();
        }

        private static Action<object, object> CompileSetter(MemberInfo member)
        {
            if (member is PropertyInfo prop && !prop.CanWrite)
            {
                var backingField = GetBackingField(prop);

                if (backingField != null)
                {
                    return backingField.SetValue;
                }
                else
                {
                    throw new ArgumentException("Property is not writable, either make it writable or add IgnoreDataMemberAttribute", nameof(member));
                }
            }

            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");
            var instanceCast = Expression.Convert(instance, member.DeclaringType);
            UnaryExpression valueCast;
            Expression setter;

            if (member is PropertyInfo property)
            {
                valueCast = Expression.Convert(value, property.PropertyType);
                setter = Expression.Call(instanceCast, property.SetMethod, valueCast);
            }
            else if (member is FieldInfo field)
            {
                valueCast = Expression.Convert(value, field.FieldType);
                setter = Expression.Assign(Expression.Field(instanceCast, field), valueCast);
            }
            else
            {
                throw new NotSupportedException("Only properties and fields are supported");
            }

            return Expression.Lambda<Action<object, object>>(setter, instance, value).Compile();
        }

        private static void Compile()
        {
            var type = typeof(T);

            var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => (m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field) && !Attribute.IsDefined(m, typeof(IgnoreDataMemberAttribute)));

            MemberMaps.Clear();
            foreach (var member in members)
            {
                CompileMember(member);
            }
        }

        private static void CompileMember(MemberInfo member)
        {

            if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field)
            {
                throw new ArgumentException("Only Fields and Properties are supported", nameof(member));
            }

            if (member.IsDefined(typeof(IgnoreDataMemberAttribute)))
            {
                throw new ArgumentException("Unable to compile properties with `IgnoreDataMemberAttribute`", nameof(member));
            }

            Type memberType;

            if (member is PropertyInfo property)
            {
                memberType = property.PropertyType;

                if (property.GetMethod.IsStatic)
                {
                    throw new ArgumentException("Member must be a instance member", nameof(member));
                }
            }
            else
            {
                FieldInfo field = member as FieldInfo;

                if (field.IsInitOnly)
                {
                    return;
                }

                memberType = field.FieldType;

                if (field.IsStatic)
                {
                    throw new ArgumentException("Member must be a instance member", nameof(member));
                }
            }

            var nullableType = Nullable.GetUnderlyingType(memberType);

            if (nullableType != null)
            {
                memberType = nullableType;
            }


            var getter = CompileGetter(member);
            var setter = CompileSetter(member);
            var converter = TypeDescriptor.GetConverter(memberType);
            MemberMaps.Add(new MemberMap(member, memberType, getter, setter, converter));
        }

        #endregion
    }
}
