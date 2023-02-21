namespace Oxide.Pooling
{
    public interface IPoolProvider
    {
        T Claim<T>() where T : class;

        T[] ClaimArray<T>(int length);

        void Unclaim<T>(ref T item) where T : class;

        IClaimable<T> GetPool<T>();
    }
}
