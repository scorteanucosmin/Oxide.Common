using System;
using System.Collections.Generic;

namespace Oxide.Pooling
{
    public sealed class ArrayPool<T> : PoolFactory, IArrayPool<T>
    {
        #region Settings / Defaults

        public const int DEFAULT_ARRAY_MAX_LENGTH = 512;

        public const int DEFAULT_ARRAY_MAX_PER_POOL = 50;

        public const bool DEFAULT_ARRAY_CLEAN_ON_RETURN = true;

        /// <summary>
        /// The amount of arrays this pool can store per array length
        /// </summary>
        public int ArraysPerLength { get; }

        /// <summary>
        /// Max length of the array this pool can store
        /// </summary>
        public int ArrayMaxLength { get; }

        /// <summary>
        /// Empties the array on return
        /// </summary>
        public bool CleanOnReturn { get; }

        #endregion

        #region Properties

        /// <summary>
        /// The shared pool instance
        /// </summary>
        public static IArrayPool<T> Shared { get; }

        /// <summary>
        /// Creates a new pool instance with default settings
        /// </summary>
        public static IArrayPool<T> Default => new ArrayPool<T>(DEFAULT_ARRAY_MAX_PER_POOL, DEFAULT_ARRAY_MAX_LENGTH, DEFAULT_ARRAY_CLEAN_ON_RETURN);

        /// <summary>
        /// Returns a pool instance that doesn't pool items
        /// </summary>
        public static IArrayPool<T> NoPool => NullArrayPool<T>.Instance;

        /// <summary>
        /// Creates a new pool instance with custom settings
        /// </summary>
        /// <param name="arrayMaxLength">Max length of the array this pool can store</param>
        /// <param name="arrayMaxPerPool">The amount of arrays this pool can store per array length</param>
        /// <param name="arrayCleanOnReturn">Empties the array on return</param>
        /// <returns>The custom array pool</returns>
        public static IArrayPool<T> Custom(int arrayMaxLength = DEFAULT_ARRAY_MAX_LENGTH, int arrayMaxPerPool = DEFAULT_ARRAY_MAX_PER_POOL, bool arrayCleanOnReturn = DEFAULT_ARRAY_CLEAN_ON_RETURN)
        {
            return new ArrayPool<T>(arrayMaxPerPool, arrayMaxLength, arrayCleanOnReturn);
        }

        public static T[] Empty { get; }

        private Dictionary<int, Stack<T[]>> Pools { get; }

        #endregion

        static ArrayPool()
        {
            Empty = [];
            Shared = Default;
        }

        private ArrayPool(int arraysPerLength = 50, int maxArrayLength = 1024, bool cleanOnReturn = true)
        {
            ArraysPerLength = arraysPerLength <= 0 ? throw new ArgumentOutOfRangeException(nameof(arraysPerLength)) : arraysPerLength;
            ArrayMaxLength = maxArrayLength < 1 ? 1 : maxArrayLength;
            CleanOnReturn = cleanOnReturn;
            Pools = new Dictionary<int, Stack<T[]>>();
        }

        public T[] Take() => Empty;

        public T[] Take(int length)
        {
            if (length <= 0)
            {
                return Empty;
            }

            T[] array;

            if (length > ArrayMaxLength)
            {
                array = new T[length];
                return array;
            }

            Stack<T[]> store;
            lock (Pools)
            {
                if (!Pools.TryGetValue(length, out store))
                {
                    array = new T[length];
                    return array;
                }
            }

            lock (store)
            {
                if (!store.TryPop(out array))
                {
                    array = new T[length];
                }
            }

            return array;
        }

        public void Return(T[] item)
        {
            if (item == null || item.Length == 0 || item.Length > ArrayMaxLength)
            {
                return;
            }

            Stack<T[]> store;
            lock (Pools)
            {
                if (!Pools.TryGetValue(item.Length, out store))
                {
                    store = new Stack<T[]>();
                    Pools[item.Length] = store;
                }
            }

            if (CleanOnReturn)
            {
                Array.Clear(item, 0, item.Length);
            }

            lock (store)
            {
                if (store.Count >= ArraysPerLength)
                {
                    return;
                }

                store.Push(item);
            }
        }

        protected override void OnPurge()
        {
            lock (Pools)
            {
                foreach (var kv in Pools)
                {
                    lock (kv.Value)
                    {
                        int total = kv.Value.Count;
                        for (int i = 0; i < total; i++)
                        {
                            T[] array = kv.Value.Pop();
                            Array.Clear(array, 0, array.Length);
                        }
                    }
                }

                Pools.Clear();
            }
        }
    }
}
