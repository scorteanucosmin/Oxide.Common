using System;
using System.Collections.Generic;

namespace Oxide.CompilerServices.Messages.Data
{
    [Serializable]
    public class CompileResult
    {
        public byte[] Assembly { get; }

        public byte[] Symbols { get; }

        public List<string> Warnings { get; } = new List<string>(); // Pooling?

        public List<string> Errors { get; } = new List<string>(); // Pooling?

        internal CompileResult(byte[] assembly, byte[] symbols)
        {
            Assembly = assembly;
            Symbols = symbols;
        }
    }
}
