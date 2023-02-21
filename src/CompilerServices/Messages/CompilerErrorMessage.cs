using Oxide.CompilerServices.Exceptions;
using System;

namespace Oxide.CompilerServices.Messages
{
    public class CompilerErrorMessage : CompilerMessage<CompilerException>
    {
        public CompilerErrorMessage(CompilerMessage source, Exception exception) : base(source.Id, MessageType.Error, new CompilerException(source, exception))
        {
        }
    }
}
