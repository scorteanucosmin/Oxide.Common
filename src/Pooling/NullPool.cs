namespace Oxide.Pooling
{
    internal class NullPool<T> : IPool<T> where T : new()
    {
        public static IPool<T> Instance { get; } = new NullPool<T>();

        public T Take() => new();

        public void Return(T item) { }
    }
}
