using System;

namespace Oxide.CompilerServices;

[Serializable]
public enum CompilerPlatform
{
    AnyCPU,
    AnyCPU32Preferred,
    Arm,
    X86,
    X64,
    IA64
}
