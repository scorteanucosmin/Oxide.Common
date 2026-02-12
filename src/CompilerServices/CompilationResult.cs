
namespace Oxide.CompilerServices;

public class CompilationResult
{
    public string Name { get; set; }
    public byte[] Data { get; set; } = [];
    public byte[] Symbols { get; set; } = [];
    public int Success;
    public int Failed;
}
