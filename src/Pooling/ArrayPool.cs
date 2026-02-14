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

        private Stack<T[]>[] Pools { get; }

        #endregion

        static ArrayPool()
        {
            Empty = new T[0];
            Shared = Default;
        }

        private ArrayPool(int arraysPerLength = 50, int maxArrayLength = 512, bool cleanOnReturn = true)
        {
            ArraysPerLength = arraysPerLength <= 0 ? throw new ArgumentOutOfRangeException(nameof(arraysPerLength)) : arraysPerLength;
            ArrayMaxLength = maxArrayLength < 1 ? 1 : maxArrayLength;
            CleanOnReturn = cleanOnReturn;

            Pools = new Stack<T[]>[ArrayMaxLength];
            for (int i = 0; i < Pools.Length; i++)
            {
                Pools[i] = new Stack<T[]>();
            }
        }

        public T[] Take() => Empty;

        public T[] Take(int length)
        {
            if (length <= 0)
            {
                return Empty;
            }

            if (length > ArrayMaxLength)
            {
                return new T[length];
            }

            Stack<T[]> store = Pools[length - 1];

            lock (store)
            {
                if (store.Count > 0)
                {
                    return store.Pop();
                }
            }

            return new T[length];
        }

        public void Return(T[] item)
        {
            if (item == null || item.Length == 0 || item.Length > ArrayMaxLength)
            {
                return;
            }

            if (CleanOnReturn)
            {
#if NETFRAMEWORK
                for (int i = 0; i < item.Length; i++) item[i] = default;
#else
                Array.Clear(item, 0, item.Length);
#endif
            }

            Stack<T[]> store = Pools[item.Length - 1];

            lock (store)
            {
                if (store.Count < ArraysPerLength)
                {
                    store.Push(item);
                }
            }
        }

        protected override void OnPurge()
        {
            for (int i = 0; i < Pools.Length; i++)
            {
                Stack<T[]> store = Pools[i];
                lock (store)
                {
                    while (store.Count > 0)
                    {
                        T[] array = store.Pop();
#if NETFRAMEWORK
                        for (int j = 0; j < array.Length; j++) array[j] = default;
#else
                        Array.Clear(array, 0, array.Length);
#endif
                    }
                }
            }
        }
    }
}
