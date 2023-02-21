using System;
using System.Collections.Generic;
using System.Linq;
using Oxide.CompilerServices.Messages.Data;

namespace Oxide.CompilerServices.Messages
{
    [Serializable]
    public class CompileRequestMessage : CompilerMessage<CompileRequest>
    {
        public CompileRequestMessage(IEnumerable<string> sources, IEnumerable<string> references, bool unsafeCompile) : base(MessageType.Request, new CompileRequest())
        {
            Data.ReferenceAssemblies.AddRange(references);
            Data.SourceFiles.AddRange(sources);
            Data.UnsafeCompile = unsafeCompile;

            if (sources.All(s => s.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
            {
                Data.Language = CodeLanguage.CSharp;
            }
            else if (sources.All(s => s.EndsWith(".vb", StringComparison.OrdinalIgnoreCase)))
            {
                Data.Language = CodeLanguage.VisualBasic;
            }
        }
    }
}
