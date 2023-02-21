using System;
using System.Collections.Generic;

namespace Oxide.CompilerServices.Messages.Data
{
    [Serializable]
    public class CompileRequest
    {
        public List<string> ReferenceAssemblies { get; } = new List<string>();

        public List<string> SourceFiles { get; } = new List<string>();

        public CodeLanguage Language { get; set; } = CodeLanguage.CSharp;

        public bool UnsafeCompile { get; set; } = true;

        public bool Debuggable { get; set; } = false;
    }
}
