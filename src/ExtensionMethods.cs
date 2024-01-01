using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Oxide.Data.Formatters;
using Oxide.Data.StorageDrivers;

namespace Oxide
{
    public static class ExtensionMethods
    {
        #region Attributes

        /// <summary>
        /// Gets a collection of attributes of a specific type
        /// </summary>
        /// <param name="provider">The <see cref="ICustomAttributeProvider"/></param>
        /// <param name="inherit">Should we search inherited attributes</param>
        /// <typeparam name="T">Will lookup <typeparamref name="T"/></typeparam>
        /// <returns>The array of <typeparamref name="T"/></returns>
        public static T[] GetCustomAttributes<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
        {
            Type a = typeof(T);
            return (T[])provider.GetCustomAttributes(a, inherit);
        }

        /// <summary>
        /// Gets a single attribute of a specific type
        /// </summary>
        /// <param name="provider">The <see cref="ICustomAttributeProvider"/></param>
        /// <param name="inherit">Should we search inherited attributes</param>
        /// <typeparam name="T">Will lookup <typeparamref name="T"/></typeparam>
        /// <returns>A single Will lookup <typeparamref name="T"/> or null</returns>
        public static T GetCustomAttribute<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
        {
            object[] attributes = provider.GetCustomAttributes(typeof(T), inherit);
            return attributes.Length > 0 ? (T)attributes[0] : null;
        }

        #endregion

        #region Storage Drivers

        /// <summary>
        /// Converts a formattable storage item into another format
        /// </summary>
        /// <param name="store">The storage driver</param>
        /// <param name="key">The key to lookup</param>
        /// <param name="context">The graph type</param>
        /// <param name="from">The format to convert from</param>
        /// <param name="to">The format to convert to</param>
        public static void ConvertTo(this IFormattableStorageDriver store, string key, Type context,
            IDataFormatter from, IDataFormatter to) => MigrateTo(store, store, key, context, from, to);

        /// <summary>
        /// Migrate data item from one <see cref="IFormattableStorageDriver"/> to another <see cref="IFormattableStorageDriver"/>
        /// </summary>
        /// <param name="source">The source storage driver</param>
        /// <param name="target">The target storage driver</param>
        /// <param name="key">The key to lookup</param>
        /// <param name="context">The graph type</param>
        /// <param name="sourceFormat">The format to convert from</param>
        /// <param name="targetFormat">The format to convert to</param>
        public static void MigrateTo(this IFormattableStorageDriver source, IFormattableStorageDriver target,
            string key, Type context, IDataFormatter sourceFormat, IDataFormatter targetFormat)
        {
            object data = source.Read(key, context, sourceFormat);
            target.Write(key, context, targetFormat);
        }

        /// <summary>
        /// Migrate data item from one <see cref="IStorageDriver"/> to a <see cref="IFormattableStorageDriver"/>
        /// </summary>
        /// <param name="source">The source storage driver</param>
        /// <param name="target">The target storage driver</param>
        /// <param name="key">The key to lookup</param>
        /// <param name="context">The graph type</param>
        /// <param name="format">The format to convert to</param>
        public static void MigrateTo(this IStorageDriver source, IFormattableStorageDriver target, string key,
            Type context, IDataFormatter format)
        {
            object data = source.Read(key, context);
            target.Write(key, data, format);
        }

        /// <summary>
        /// Migrate data item from one <see cref="IStorageDriver"/> to a <see cref="IStorageDriver"/>
        /// </summary>
        /// <param name="source">The source storage driver</param>
        /// <param name="target">The target storage driver</param>
        /// <param name="key">The key to lookup</param>
        /// <param name="context">The graph type</param>
        public static void MigrateTo(this IStorageDriver source, IStorageDriver target, string key, Type context)
        {
            object data = source.Read(key, context);
            target.Write(key, data);
        }

        #endregion

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

        /// <summary>
        /// Converts a IEnumerable into a <see cref="HashSet{T}"/>
        /// </summary>
        /// <param name="collection">The collection</param>
        /// <typeparam name="T">The collection type</typeparam>
        /// <returns>A populated <see cref="HashSet{T}"/></returns>
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

        #endregion
    }
}
