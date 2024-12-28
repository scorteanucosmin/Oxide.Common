using System;
using System.Runtime.InteropServices;

#if NETSTANDARD2_1_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Oxide.IO.Serialization.Binary
{
    /// <summary>
    /// Provides a serializer and deserializer for primitive and blittable value types.
    /// </summary>
    /// <typeparam name="T">The type to be serialized or deserialized. Must be a struct and preferably blittable.</typeparam>
    public sealed class PrimitiveTypeSerializer<T> : IBinaryWriter<T>, IBinaryReader<T> where T : struct
    {
        /// <summary>
        /// The type being serialized or deserialized.
        /// </summary>
        private static Type Type { get; }

        /// <summary>
        /// The fixed size, in bytes, of the type <typeparamref name="T"/>.
        /// </summary>
        private static int FixedSize { get; }

        static PrimitiveTypeSerializer()
        {
            Type type = typeof(T);

            if (!type.IsValueType)
            {
                throw new NotSupportedException($"{type} is not blittable");
            }

            Type = type;
            try
            {
#if NETSTANDARD2_1_OR_GREATER
                FixedSize = Unsafe.SizeOf<T>();
#else
                FixedSize = Marshal.SizeOf(Type);
#endif
            }
            catch
            {
                throw new NotSupportedException($"{type} is not blittable");
            }
        }

        /// <inheritdoc />
        public bool CanWrite(T instance, ReadOnlySpan<byte> buffer) => FixedSize <= buffer.Length;

        /// <inheritdoc />
        public int Write(T instance, Span<byte> buffer)
        {
            if (FixedSize > buffer.Length)
            {
                throw new ArgumentException($"Output buffer is too small to write {Type}, Required: {FixedSize} | Actual: {buffer.Length}");
            }

#if NETSTANDARD2_1_OR_GREATER
            Span<byte> instanceData = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref instance, 1));
            instanceData.CopyTo(buffer);
#else
            GCHandle handle = GCHandle.Alloc(instance, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                Span<byte> source;

                unsafe
                {
                    source = new Span<byte>(ptr.ToPointer(), FixedSize);
                }

                source.CopyTo(buffer);
            }
            finally
            {
                handle.Free();
            }
#endif
            return FixedSize;
        }

        /// <inheritdoc />
        public bool CanRead(ReadOnlySpan<byte> buffer) => FixedSize <= buffer.Length;

        /// <inheritdoc />
        public int Read(ReadOnlySpan<byte> buffer, ref T instance)
        {
            if (FixedSize > buffer.Length)
            {
                throw new ArgumentException($"Input buffer is too small to read {Type}, Required: {FixedSize} | Actual: {buffer.Length}");
            }

            buffer = buffer[..FixedSize];

#if NETSTANDARD2_1_OR_GREATER
            Span<byte> targetSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref instance, 1));
            buffer.CopyTo(targetSpan);
#else
            
            GCHandle handle = GCHandle.Alloc(instance, GCHandleType.Pinned);

            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();

                unsafe
                {
                    Span<byte> target = new Span<byte>(ptr.ToPointer(), FixedSize);
                    buffer.CopyTo(target);
                }
            }
            finally
            {
                handle.Free();
            }
#endif
            return FixedSize;
        }
    }
}
