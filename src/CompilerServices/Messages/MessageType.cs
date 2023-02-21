using System;

namespace Oxide.CompilerServices.Messages
{
    [Serializable]
    public enum MessageType
    {
        None = 0x00,
        Request = 0x01,
        Result = 0x02,
        Error = 0x04
    }
}
