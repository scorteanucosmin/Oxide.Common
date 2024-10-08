using System;
using System.Collections.Generic;

namespace Oxide.Pooling
{
    public abstract class PoolFactory : IDisposable
    {
        private bool DisposedValue { get; set; }

        /// <summary>
        /// Pools will subscribe to this to purge all existing pools managed by this factory
        /// </summary>
        private static event Action OnPurgeStart;

        /// <summary>
        /// Purges all pools managed by this factory
        /// </summary>
        /// <returns>Total amount of bytes freed from memory</returns>
        public static long Purge()
        {
            if (OnPurgeStart == null)
            {
                return 0;
            }

            // Force initial cleanup allowing GC to identify objects needing cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryBefore = GC.GetTotalMemory(forceFullCollection: true);

            OnPurgeStart();

            // Force another cleanup for all the items just purged
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryAfter = GC.GetTotalMemory(forceFullCollection: true);
            return memoryBefore - memoryAfter;
        }

        protected PoolFactory()
        {
            OnPurgeStart += OnPurge;
        }

        ~PoolFactory() => Dispose(false);

        protected virtual void OnPurge()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                DisposedValue = true;
                if (disposing)
                {
                    OnPurge();
                }

                OnPurgeStart -= OnPurge;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public sealed class PoolFactory<T> : PoolFactory, IPool<T> where T : new()
    {
        #region Settings / Defaults

        public const int DEFAULT_MAX_POOL_SIZE = 256;

        public const bool DEFAULT_ITEM_DISPOSE = false;

        #endregion

        #region Properties

        /// <summary>
        /// A shared pool with default settings
        /// </summary>
        public static IPool<T> Shared { get; }

        /// <summary>
        /// Returns a new pool with default settings
        /// </summary>
        public static IPool<T> Default => new PoolFactory<T>(DEFAULT_MAX_POOL_SIZE, DEFAULT_ITEM_DISPOSE);

        /// <summary>
        /// Returns a pool instance that doesn't pool items
        /// </summary>
        public static IPool<T> NoPool => NullPool<T>.Instance;

        /// <summary>
        /// Creates a new pool with custom settings
        /// </summary>
        /// <param name="maxPoolSize">Max items allowed to pool</param>
        /// <param name="itemDispose">Calls dispose on items when they get returned to the pool</param>
        /// <returns>The pool provider</returns>
        public static IPool<T> Custom(int maxPoolSize = DEFAULT_MAX_POOL_SIZE, bool itemDispose = DEFAULT_ITEM_DISPOSE) => new PoolFactory<T>(maxPoolSize, itemDispose);

        static PoolFactory()
        {
            Shared = Default;
        }

        #endregion

        private Stack<T> Store { get; }

        private int MaxPoolSize { get; }

        private bool CallItemDispose { get; }

        private PoolFactory(int maxPooledSize = 50, bool callItemDispose = false)
        {
            MaxPoolSize = maxPooledSize <= 0 ? throw new ArgumentOutOfRangeException(nameof(maxPooledSize)) : maxPooledSize;
            CallItemDispose = callItemDispose;
            Store = new Stack<T>();
        }

        public T Take()
        {
            lock (Store)
            {
                if (Store.Count > 0)
                {
                    return Store.Pop();
                }
            }

            return new();
        }

        public void Return(T item)
        {
            if (CallItemDispose && item is IDisposable dispose)
            {
                dispose.Dispose();
            }

            lock (Store)
            {
                if (Store.Count >= MaxPoolSize)
                {
                    return;
                }

                Store.Push(item);
            }
        }

        protected override void OnPurge()
        {
            lock (Store)
            {
                while (Store.Count > 0)
                {
                    T item = Store.Pop();

                    if (item is IDisposable dispose)
                    {
                        dispose.Dispose();
                    }
                }
            }
        }
    }
}
