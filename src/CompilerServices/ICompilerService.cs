using System;
using Oxide.Converters;

namespace Oxide.CompilerServices
{
    public interface ICompilerService<T> : IDisposable
    {
        IConverter<ICompilation, T> Converter { get; }
    }
}
