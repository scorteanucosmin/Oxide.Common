using System.Collections.Generic;

namespace Oxide.CompilerServices;

public class CompilerMessage
{
    public int Id { get; set; }

    public MessageType Type { get; set; }

    public byte[] Data { get; set; }

    public List<CompilerError>? Errors { get; set; }
}
