using System;

namespace Oxide.IO
{
    [Flags]
    public enum NotifyMask : uint
    {
        Default = OnCreated | OnDeleted | OnModified | Moved,
        All = OnCreated | OnDeleted | OnModified | OnOpen | OnAccess | OnAttributeChange | OnMovedFrom | OnMovedTo | WriteClosed | ReadClosed,
        Moved = OnMovedFrom | OnMovedTo,
        Closed = WriteClosed | ReadClosed,
        OnCreated = 0x00000100,
        OnDeleted = 0x00000200,
        OnModified = 0x00000002,
        OnOpen = 0x00000020,
        OnAccess = 0x00000001,
        OnAttributeChange = 0x00000004,
        OnMovedFrom = 0x00000040,
        OnMovedTo = 0x00000080,
        WriteClosed = 0x00000008,
        ReadClosed = 0x00000010,
        DirectoryOnly = 0x40000000
    }
}
