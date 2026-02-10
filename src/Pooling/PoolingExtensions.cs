using System;

namespace Oxide.Pooling
{
    public static class PoolingExtensions
    {
        #region Arrays

        /// <summary>
        /// Copies the source array to a pooled array of the same type
        /// </summary>
        /// <param name="provider">The pool provider to get the pooled array from</param>
        /// <param name="source">The source array containing the <see cref="T"/>'s to copy</param>
        /// <param name="offset">The offset to start copying from the source array</param>
        /// <param name="count">The total items to copy from the source array</param>
        /// <typeparam name="T">The element type of the array</typeparam>
        /// <returns>A array of <see cref="T"/> with the length of the total elements copied</returns>
        /// <exception cref="System.ArgumentNullException">If the provider or the source array is null</exception>
        /// <exception cref="System.IndexOutOfRangeException">If the copy process tries to read outside the bounds of the source array</exception>
        public static T[] PooledCopy<T>(this IArrayPool<T> provider, T[] source, int offset, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            T[] copy = provider?.Take(count) ?? throw new ArgumentNullException(nameof(provider));
            try
            {
                for (int i = 0; i < copy.Length; i++)
                {
                    copy[i] = source[offset];
                    offset += 1;
                }

                return copy;
            }
            catch
            {
                provider.Return(copy);
                throw;
            }
        }

        #endregion
    }
}
