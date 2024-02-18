namespace Oxide.IO
{
    public struct FileSystemEvent
    {
        public string Directory { get; }

        public string Name { get; }

        public NotifyMask Event { get; }

        public FileSystemEvent(string directory, string name, NotifyMask eventType)
        {
            Directory = directory;
            Name = name;
            Event = eventType;
        }
    }
}
