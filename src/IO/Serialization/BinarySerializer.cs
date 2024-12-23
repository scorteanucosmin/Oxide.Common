using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace Oxide.IO.Serialization
{
    public sealed class BinarySerializer<T> : ISerializer<T, byte>, IDeserializer<T, byte> where T : class
    {
        private static Dictionary<MemberInfo, KeyValuePair<Func<object, object>, Action<object, object>>> GettersSetters { get; }

        private static Dictionary<MemberInfo, TypeConverter> ConverterCache { get; }

        private static List<MemberInfo> SerializableMembers { get; }

        private Encoding Encoder { get; }

        #region Constructor & Dispose

        static BinarySerializer()
        {
            GettersSetters = new Dictionary<MemberInfo, KeyValuePair<Func<object, object>, Action<object, object>>>();
            SerializableMembers = new List<MemberInfo>();
            ConverterCache = new Dictionary<MemberInfo, TypeConverter>();
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

            int offset = 0;
            foreach (MemberInfo member in SerializableMembers)
            {
                object val = GetValue(member, obj);
                TypeConverter converter = GetCachedConverter(member);

                if (converter.CanConvertTo(typeof(string)))
                {
                    string serializedValue = val == null ? string.Empty : converter.ConvertToInvariantString(val) ?? string.Empty;
                    int requiredBytes = Encoder.GetByteCount(serializedValue);

                    if (offset + requiredBytes > outputBuffer.Length)
                    {
                        throw new ArgumentException($"Output buffer is too small. Required: {offset + requiredBytes}, Available: {outputBuffer.Length}");
                    }

                    outputBuffer.WriteLengthPrefix(requiredBytes, ref offset);
                    int encodedData = Encoder.GetBytes(serializedValue.AsSpan(), outputBuffer.Slice(offset));
                    offset += encodedData;
                }
                else
                {
                    throw new NotSupportedException($"Member {member} does not have a TypeConverter that converts to string");
                }
            }

            return offset;
        }

        #endregion

        #region Deserializer


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize(ReadOnlySpan<byte> inputBuffer, T existingInstance = null)
        {
            existingInstance ??= Activator.CreateInstance<T>();
            int offset = 0;

            foreach (MemberInfo member in SerializableMembers)
            {
                TypeConverter converter = GetCachedConverter(member);
                int strLen = inputBuffer.ToInt32(offset);
                offset += sizeof(int);
                string serializedValue = strLen > 0
                    ? Encoder.GetString(inputBuffer.Slice(offset, strLen))
                    : string.Empty;
                offset += strLen;

                object val = null;

                if (!string.IsNullOrEmpty(serializedValue))
                {
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        val = converter.ConvertFromInvariantString(serializedValue);
                    }
                    else
                    {
                        throw new NotSupportedException($"Member {member} does not have a TypeConverter that converts from string");
                    }
                }

                SetValue(member, existingInstance, val);
            }

            return existingInstance;
        }

        #endregion

        #region Helpers

        private static object GetValue(MemberInfo member, T obj)
        {
            if (!GettersSetters.TryGetValue(member, out var gs))
            {
                CompileMember(member);
                return GetValue(member, obj);
            }

            return gs.Key(obj);
        }

        private static void SetValue(MemberInfo member, T obj, object value)
        {
            if (!GettersSetters.TryGetValue(member, out var gs))
            {
                CompileMember(member);
                SetValue(member, obj, value);
                return;
            }

            gs.Value(obj, value);
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

        private static TypeConverter GetCachedConverter(MemberInfo member)
        {
            if (!ConverterCache.TryGetValue(member, out TypeConverter converter))
            {
                Type memberType = GetMemberType(member);
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

        private static Func<object, object> CompileGetter(MemberInfo member)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            UnaryExpression instanceCast = Expression.Convert(instance, member.DeclaringType);

            Expression memberAccess = member switch
            {
                PropertyInfo property => Expression.Property(instanceCast, property),
                FieldInfo field => Expression.Field(instanceCast, field),
                _ => throw new NotSupportedException($"Unsupported member type: {member.MemberType}")
            };

            UnaryExpression convert = Expression.Convert(memberAccess, typeof(object));
            return Expression.Lambda<Func<object, object>>(convert, instance).Compile();
        }

        private static Action<object, object> CompileSetter(MemberInfo member)
        {
            if (member is PropertyInfo prop && !prop.CanWrite)
            {
                FieldInfo backingField = GetBackingField(prop);

                if (backingField != null)
                {
                    return backingField.SetValue;
                }
                else
                {
                    throw new ArgumentException("Property is not writable, either make it writable or add IgnoreDataMemberAttribute", nameof(member));
                }
            }

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression value = Expression.Parameter(typeof(object), "value");
            UnaryExpression instanceCast = Expression.Convert(instance, member.DeclaringType);
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

        public static void Compile()
        {
            Type type = typeof(T);

            IEnumerable<MemberInfo> members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => (m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field) && !Attribute.IsDefined(m, typeof(IgnoreDataMemberAttribute)));

            ConverterCache.Clear();
            SerializableMembers.Clear();

            foreach (MemberInfo member in members)
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

            
            Func<object, object> getter = CompileGetter(member);
            Action<object, object> setter = CompileSetter(member);
            GettersSetters[member] = new(getter, setter);
            ConverterCache[member] = TypeDescriptor.GetConverter(memberType);

            if (!SerializableMembers.Contains(member))
            {
                SerializableMembers.Add(member);
            }
        }

        #endregion
    }
}
