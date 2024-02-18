namespace Oxide.IO
{
    public interface IFileSystem
    {
        bool IsSymbolicLink(string path);

        string ResolvePath(string path);
    }
}
