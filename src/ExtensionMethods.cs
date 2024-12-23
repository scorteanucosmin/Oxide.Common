using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Oxide
{
    public static class ExtensionMethods
    {
        #region Metadata

        /// <summary>
        /// Gets all <see cref="AssemblyMetadataAttribute"/> from a assembly
        /// </summary>
        /// <param name="assembly">The assembly to query for metadata</param>
        /// <returns>Array of metadata</returns>
        public static AssemblyMetadataAttribute[] Metadata(this Assembly assembly)
        {
            return Attribute.GetCustomAttributes(assembly, typeof(AssemblyMetadataAttribute), false) as AssemblyMetadataAttribute[];
        }

        /// <summary>
        /// Gets values of all <see cref="AssemblyMetadataAttribute"/> filtered by key
        /// </summary>
        /// <param name="assembly">The assembly to query for metadata</param>
        /// <param name="key">The metadata key to filter by</param>
        /// <returns>Array of metadata values</returns>
        public static string[] Metadata(this Assembly assembly, string key)
        {
            return assembly.Metadata().Where(meta => meta.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)).Select(meta => meta.Value).ToArray();
        }

        #endregion

        #region Enumberables

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        {
            HashSet<T> set = new HashSet<T>();

            foreach (T item in collection)
            {
                set.Add(item);
            }

            return set;
        }

        #endregion

        #region Conversions

        public static int WriteBigEndian(this int value, byte[] array, int startPos = 0)
        {
            array[startPos]     = (byte)(value >> 24);
            array[startPos + 1] = (byte)(value >> 16);
            array[startPos + 2] = (byte)(value >> 8);
            array[startPos + 3] = (byte)value;
            return sizeof(int);
        }

        public static int ReadBigEndian(this byte[] array, int startPos = 0)
        {
            return (array[startPos] << 24) | (array[startPos + 1] << 16) | (array[startPos + 2] << 8) | array[startPos + 3];
        }

        public static int WriteLittleEndian(this int value, byte[] array, int startPos = 0)
        {
            array[startPos] = (byte)value;
            array[startPos + 1] = (byte)(value >> 8);
            array[startPos + 2] = (byte)(value >> 16);
            array[startPos + 3] = (byte)(value >> 24);
            return sizeof(int);
        }

        public static int ReadLittleEndian(this byte[] array, int startPos = 0)
        {
            return array[startPos] | (array[startPos + 1] << 8) | (array[startPos + 2] << 16) | (array[startPos + 3] << 24);
        }

        #endregion

        #region Collections

#if NETFRAMEWORK || NETSTANDARD2_0

        // Stack<T>
        public static bool TryPop<T>(this Stack<T> stack, out T value)
        {
            if (stack.Count > 0)
            {
                value = stack.Pop();
                return true;
            }

            value = default;
            return false;
        }

        // Stack<T>
        public static bool TryPeek<T>(this Stack<T> stack, out T value)
        {
            if (stack.Count > 0)
            {
                value = stack.Peek();
                return true;
            }

            value = default;
            return false;
        }

        // Queue<T>
        public static bool TryDequeue<T>(this Queue<T> queue, out T value)
        {
            if (queue.Count > 0)
            {
                value = queue.Dequeue();
                return true;
            }

            value = default;
            return false;
        }

        // Queue<T>
        public static bool TryPeek<T>(this Queue<T> queue, out T value)
        {
            if (queue.Count > 0)
            {
                value = queue.Peek();
                return true;
            }

            value = default;
            return false;
        }

#endif

        #endregion

        #region Spans

#if NET48

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this MemoryStream stream, ReadOnlySpan<byte> buffer)
        {
            int currentPosition = (int)stream.Position;
            int requiredSize = currentPosition + buffer.Length;

            if (stream.Capacity < requiredSize)
            {
                stream.Capacity = requiredSize;
            }

            buffer.CopyTo(stream.GetBuffer().AsSpan(currentPosition, buffer.Length));
            stream.Position += buffer.Length;

            if (requiredSize > stream.Length)
            {
                stream.SetLength(requiredSize);
            }
        }

        public static int GetBytes(this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            if (chars.IsEmpty)
            {
                return 0;
            }

            unsafe
            {
                fixed (char* charPtr = chars)
                fixed (byte* bytePtr = bytes)
                {
                    return encoding.GetBytes(charPtr, chars.Length, bytePtr, bytes.Length);
                }
            }
        }

        public static string GetString(this Encoding encoding, ReadOnlySpan<byte> span)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            if (span.IsEmpty)
            {
                return string.Empty;
            }

            unsafe
            {
                fixed (byte* bytePtr = span)
                {
                    return encoding.GetString(bytePtr, span.Length);
                }
            }
        }

        #endif

        public static int ToInt32(this ReadOnlySpan<byte> span, int offset = 0)
        {
            if (offset + sizeof(int) > span.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "Not enough data in span to read an integer");
            }

            unsafe
            {
                fixed (byte* ptr = span.Slice(offset))
                {
                    return *(int*)ptr;
                }
            }
        }

        public static void WriteLengthPrefix(this Span<byte> span, int value, ref int offset)
        {
            if (offset + sizeof(int) > span.Length)
            {
                throw new ArgumentException("Not enough space in span to write length prefix", nameof(span));
            }

            unsafe
            {
                fixed (byte* ptr = span.Slice(offset, sizeof(int)))
                {
                    *(int*)ptr = value;
                }
            }

            offset += sizeof(int);
        }

        #endregion
    }
}
