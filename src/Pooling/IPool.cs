namespace Oxide.Pooling
{
    /// <summary>
    /// Used for pooling items
    /// </summary>
    /// <typeparam name="T">The item type this pool manages</typeparam>
    public interface IPool<T>
    {
        /// <summary>
        /// Takes a single <see cref="T"/> from this pool
        /// </summary>
        /// <returns><see cref="T"/></returns>
        T Take();

        /// <summary>
        /// Returns a item to the pool
        /// </summary>
        /// <param name="item">The item to return</param>
        void Return(T item);
    }
}
