using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Oxide.Pooling;

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

        public static string JoinValues(this IList<string> collection, char separator)
        {
            int listCount = collection.Count;
            if (listCount == 0)
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = PoolFactory<StringBuilder>.Shared.Take();
            try
            {
                int maxIndex = listCount - 1;
                for (int i = 0; i < listCount; i++)
                {
                    string item = collection[i];
                    stringBuilder.Append(item);

                    if (i >= maxIndex)
                    {
                        continue;
                    }

                    stringBuilder.Append(separator);
                }

                return stringBuilder.ToString();
            }
            finally
            {
                stringBuilder.Length = 0;
                PoolFactory<StringBuilder>.Shared.Return(stringBuilder);
            }
        }

        public static string JoinValues(this IList<string> collection, string separator)
        {
            int collectionCount = collection.Count;
            if (collectionCount == 0)
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = PoolFactory<StringBuilder>.Shared.Take();
            try
            {
                int maxIndex = collectionCount - 1;
                for (int i = 0; i < collectionCount; i++)
                {
                    string item = collection[i];
                    stringBuilder.Append(item);

                    if (i >= maxIndex)
                    {
                        continue;
                    }

                    stringBuilder.Append(separator);
                }

                return stringBuilder.ToString();
            }
            finally
            {
                stringBuilder.Length = 0;
                PoolFactory<StringBuilder>.Shared.Return(stringBuilder);
            }
        }

        public static string JoinValues(this HashSet<string> collection, string separator)
        {
            int collectionCount = collection.Count;
            if (collectionCount == 0)
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = PoolFactory<StringBuilder>.Shared.Take();
            try
            {
                int index = 0;
                foreach (string item in collection)
                {
                    index++;

                    stringBuilder.Append(item);

                    if (index >= collectionCount)
                    {
                        continue;
                    }

                    stringBuilder.Append(separator);
                }

                return stringBuilder.ToString();
            }
            finally
            {
                stringBuilder.Length = 0;
                PoolFactory<StringBuilder>.Shared.Return(stringBuilder);
            }
        }

        public static string JoinValues(this HashSet<string> collection, char separator)
        {
            int collectionCount = collection.Count;
            if (collectionCount == 0)
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = PoolFactory<StringBuilder>.Shared.Take();
            try
            {
                int index = 0;
                foreach (string item in collection)
                {
                    index++;

                    stringBuilder.Append(item);

                    if (index >= collectionCount)
                    {
                        continue;
                    }

                    stringBuilder.Append(separator);
                }

                return stringBuilder.ToString();
            }
            finally
            {
                stringBuilder.Length = 0;
                PoolFactory<StringBuilder>.Shared.Return(stringBuilder);
            }
        }

        #endregion
    }
}
