namespace Oxide.Pooling
{
    /// <summary>
    /// A interface used for managing pooled items
    /// </summary>
    public interface IPoolProvider
    {
        /// <summary>
        /// Returns a item to the pool
        /// </summary>
        /// <param name="item">The item to return</param>
        void Return(object item);
    }

    /// <inheritdoc cref="IPoolProvider"/>
    /// <typeparam name="T">The item type this pool manages</typeparam>
    public interface IPoolProvider<out T> : IPoolProvider
    {
        /// <summary>
        /// Takes a single <see cref="T"/> from this pool
        /// </summary>
        /// <returns><see cref="T"/></returns>
        T Take();
    }
}
