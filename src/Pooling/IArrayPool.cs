namespace Oxide.Pooling
{
    /// <inheritdoc cref="IPool{T[]}"/>
    /// <typeparam name="T">The item type this pool manages</typeparam>
    public interface IArrayPool<T> : IPool<T[]>
    {
        /// <summary>
        /// Takes a single <see cref="T"/> array from this pool
        /// </summary>
        /// <param name="length"></param>
        /// <returns><see cref="T"/>[]</returns>
        T[] Take(int length);
    }
}
