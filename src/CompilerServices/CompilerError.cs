namespace Oxide.CompilerServices;

public class CompilerError
{
    public string Message { get; set; }

    public string File { get; set; }

    public int Line { get; set; }

    public int Position { get; set; }
}
