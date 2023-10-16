using System;

namespace Oxide.Pooling
{
    /// <summary>
    /// A factory interface used for managing registered <see cref="IPoolProvider"/>'s
    /// </summary>
    public interface IPoolFactory
    {
        /// <summary>
        /// Get the pooling provider for a specific type
        /// </summary>
        /// <typeparam name="T">The handled type</typeparam>
        /// <returns>The <see cref="IPoolProvider"/> that handles the requested type</returns>
        IPoolProvider<T> GetProvider<T>();

        /// <summary>
        /// Check if this factory has registered <see cref="IPoolProvider"/> for a type
        /// </summary>
        /// <typeparam name="T">The type to check for</typeparam>
        /// <returns>Will return true if a <see cref="IPoolProvider"/> is registered</returns>
        bool IsHandledType<T>();

        /// <summary>
        /// Registers a <see cref="IPoolProvider"/> with this factory
        /// </summary>
        /// <param name="provider">The created provider</param>
        /// <param name="args">Additional parameters used to instantiate the <see cref="IPoolProvider"/></param>
        /// <typeparam name="TProvider">The <see cref="IPoolProvider"/></typeparam>
        /// <returns>A disposable object that will unregister the <see cref="IPoolProvider"/> when disposed</returns>
        IDisposable RegisterProvider<TProvider>(out TProvider provider, params object[] args) where TProvider : IPoolProvider;
    }
}
