using System;
using System.ComponentModel;

namespace Oxide.IO
{
    public interface IFileSystemWatcher : IObservable<FileSystemEvent>, ISupportInitialize, IDisposable
    {
        string Directory { get; }

        bool IncludeSubDirectories { get; }

        NotifyMask Filter { get; }

        bool IsMonitoredDirectory(string directory);

        IFileSystemWatcher ClearFilters();

        IFileSystemWatcher Ignore(string pattern);
    }
}
