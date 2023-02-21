using System;

namespace Oxide.Pooling
{
    public static class Pool
    {
        public static IPoolProvider Provider { get; }

        static Pool()
        {
            Type type = Type.GetType("Oxide.Pooling.DynamicPoolProvider", true);
            Provider = (IPoolProvider)Activator.CreateInstance(type);
        }

        public static T Claim<T>() where T : class => Provider.Claim<T>();

        public static T[] ClaimArray<T>(int length) => Provider.ClaimArray<T>(length);

        public static void Unclaim<T>(ref T item) where T : class => Provider.Unclaim(ref item);
    }
}
