namespace Oxide.Pooling
{
    /// <summary>
    /// Implement this interface on pooled types to automatically reset state when returned to a pool.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Resets the object to a clean state for reuse from the pool
        /// </summary>
        void Reset();
    }
}
