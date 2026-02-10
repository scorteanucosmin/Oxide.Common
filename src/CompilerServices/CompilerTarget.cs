using System;

namespace Oxide.CompilerServices;

[Serializable]
public enum CompilerTarget
{
    Library,
    Exe,
    Module,
    WinExe
}
