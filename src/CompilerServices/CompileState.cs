using System;

namespace Oxide.CompilerServices
{
    [Flags]
    public enum CompileState
    {
        Unknown = 0x00,
        Queued = 0x01,
        Started = 0x02,
        Stalled = 0x04,
        Complete = 0x08
    }
}
