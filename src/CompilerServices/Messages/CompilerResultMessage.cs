using Oxide.CompilerServices.Messages.Data;

namespace Oxide.CompilerServices.Messages
{
    public class CompilerResultMessage : CompilerMessage<CompileResult>
    {
        public CompilerResultMessage(CompileRequestMessage message, byte[] assembly, byte[] symbols) : base(message.Id, MessageType.Result, new CompileResult(assembly, symbols))
        {
        }
    }
}
