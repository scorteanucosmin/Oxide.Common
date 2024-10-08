namespace Oxide.Pooling
{
    internal class NullArrayPool<T> : IArrayPool<T>
    {
        public static IArrayPool<T> Instance { get; } = new NullArrayPool<T>();

        protected T[] Empty { get; }

        private NullArrayPool()
        {
            Empty = [];
        }

        public void Return(T[] item) { }

        public T[] Take(int length) => new T[length];

        public T[] Take() => Empty;
    }
}
