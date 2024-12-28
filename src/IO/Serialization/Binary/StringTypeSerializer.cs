using System;
using Oxide.Pooling;
using System.Text;
using System.Runtime.InteropServices;

namespace Oxide.IO.Serialization.Binary
{
    /// <summary>
    /// Provides serialization and deserialization methods for strings using a specified encoding.
    /// </summary>
    public class StringTypeSerializer : IBinaryWriter<string>, IBinaryReader<string>
    {
        /// <summary>
        /// The default encoding used for string serialization and deserialization.
        /// </summary>
        private static Encoding DefaultEncoding { get; }

        static StringTypeSerializer()
        {
            DefaultEncoding = Encoding.UTF8;
        }

        /// <summary>
        /// The encoding used for this instance of the serializer.
        /// </summary>
        private Encoding Encoding { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringTypeSerializer"/> class with the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use for serialization. Defaults to UTF-8 if not specified.</param>
        public StringTypeSerializer(Encoding encoding = null)
        {
            Encoding = encoding ?? DefaultEncoding;
        }

        /// <summary>
        /// Determines whether the buffer contains sufficient data to deserialize a string.
        /// </summary>
        /// <param name="buffer">The buffer containing serialized data.</param>
        /// <returns>True if the buffer has at least 4 bytes (length prefix) to start deserialization.</returns>
        public bool CanRead(ReadOnlySpan<byte> buffer)
        {
            return buffer.Length >= sizeof(int);
        }

        /// <summary>
        /// Determines whether the buffer has sufficient space to write the serialized string.
        /// </summary>
        /// <param name="instance">The string to serialize.</param>
        /// <param name="buffer">The buffer to write into.</param>
        /// <returns>
        /// <c>true</c> if the buffer has enough space to hold the serialized string; otherwise, <c>false</c>.
        /// </returns>
        public bool CanWrite(string instance, ReadOnlySpan<byte> buffer)
        {
            int size = instance == null ? sizeof(int) : Encoding.GetByteCount(instance) + sizeof(int);
            return buffer.Length >= size;
        }

        /// <summary>
        /// Reads a string from the buffer using the specified encoding.
        /// </summary>
        /// <param name="buffer">The buffer containing the serialized string.</param>
        /// <param name="instance">
        /// A reference to the string instance to populate with the deserialized data.
        /// </param>
        /// <returns>The number of bytes read from the buffer.</returns>
        public int Read(ReadOnlySpan<byte> buffer, ref string instance)
        {
            if (buffer.Length < sizeof(int))
            {
                throw new ArgumentException("Buffer is too small to contain a length prefix.", nameof(buffer));
            }


            int stringLength;
#if NETSTANDARD2_1_OR_GREATER
            stringLength = BitConverter.ToInt32(buffer.Slice(0, sizeof(int)));
#else
            byte[] lengthBytes = ArrayPool<byte>.Shared.Take(sizeof(int));
            try
            {
                buffer[..sizeof(int)].CopyTo(lengthBytes);
                stringLength = BitConverter.ToInt32(lengthBytes, 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(lengthBytes);
            }
#endif

            buffer = buffer[sizeof(int)..];

            if (buffer.Length < stringLength)
            {
                throw new ArgumentException("Buffer is too small to contain the serialized string.", nameof(buffer));
            }

#if NETSTANDARD2_1_OR_GREATER
            instance = Encoding.GetString(buffer[..stringLength]);
#else
            byte[] stringBytes = ArrayPool<byte>.Shared.Take(stringLength);

            try
            {
                buffer[..stringLength].CopyTo(stringBytes);
                instance = Encoding.GetString(stringBytes);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(stringBytes);
            }
#endif
            return stringLength + sizeof(int);
        }

        /// <summary>
        /// Writes a string with a length prefix to the buffer using the specified encoding.
        /// </summary>
        /// <param name="instance">The string to serialize.</param>
        /// <param name="buffer">The buffer to write into.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        /// <exception cref="ArgumentException">Thrown if the buffer is too small to hold the serialized string and its length prefix.</exception>
        public int Write(string instance, Span<byte> buffer)
        {
            int stringLength = instance == null ? 0 : Encoding.GetByteCount(instance);
            int requiredBytes = sizeof(int) + stringLength;

            if (buffer.Length < requiredBytes)
            {
                throw new ArgumentException($"Buffer is too small. Required: {requiredBytes}, Available: {buffer.Length}");
            }

            Span<byte> lengthPrefix = buffer[..sizeof(int)];

#if NETSTANDARD2_1_OR_GREATER
            MemoryMarshal.Write(lengthPrefix, ref stringLength);
#else
            unsafe
            {
                fixed (byte* ptr = lengthPrefix)
                {
                    *(int*)ptr = stringLength;
                }
            }
#endif

            if (stringLength == 0)
            {
                return sizeof(int);
            }

            buffer = buffer[sizeof(int)..];
            ReadOnlySpan<char> stringSpan = instance.AsSpan();

#if NETSTANDARD2_1_OR_GREATER
            Encoding.GetBytes(stringSpan, buffer);
#else
            unsafe
            {
                fixed (char* charPtr = stringSpan)
                fixed (byte* bytePtr = buffer)
                {
                    Encoding.GetBytes(charPtr, stringSpan.Length, bytePtr, buffer.Length);
                }
            }
#endif
            return requiredBytes;
        }
    }
}
