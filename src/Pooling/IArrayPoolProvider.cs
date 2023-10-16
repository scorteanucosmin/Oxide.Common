namespace Oxide.Pooling
{
    /// <inheritdoc cref="IPoolProvider"/>
    /// <typeparam name="T">The item type this pool manages</typeparam>
    public interface IArrayPoolProvider<out T> : IPoolProvider<T[]>
    {
        /// <summary>
        /// Takes a single <see cref="T"/> array from this pool
        /// </summary>
        /// <param name="length"></param>
        /// <returns><see cref="T"/>[]</returns>
        T[] Take(int length);
    }
}
