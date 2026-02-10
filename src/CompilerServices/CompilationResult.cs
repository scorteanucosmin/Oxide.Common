using System;

namespace Oxide.CompilerServices;

[Serializable]
public class CompilationResult
{
    public string Name { get; set; }
    public byte[] Data { get; set; } = [];
    public byte[] Symbols { get; set; } = [];

    [NonSerialized]
    public int Success;

    [NonSerialized]
    public int Failed;
}
