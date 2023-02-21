using Oxide.CompilerServices.Messages;
using System;

namespace Oxide.CompilerServices.Exceptions
{
    [Serializable]
    public class CompilerException : Exception
    {
        public Guid MessageId { get; }

        public CompilerException(CompilerMessage message, Exception exception) : base($"Compiler threw a exception during {message.Type}Request: {message.Id}", exception) 
        {
            MessageId = message.Id;
        }
    }
}
