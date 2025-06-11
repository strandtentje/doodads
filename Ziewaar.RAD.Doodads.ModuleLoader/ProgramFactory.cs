#nullable enable
using Ziewaar.RAD.Doodads.RKOP.SeriesParsers;

namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class FileWatcherFactory : IDisposable
{
    public static readonly FileWatcherFactory Instance = new();
    private readonly List<FileSystemWatcher> ActiveWatchers = new();

    public void Dispose()
    {
        foreach (var item in ActiveWatchers)
        {
            StopOne(item);
        }
    }
    private static void StopOne(FileSystemWatcher item)
    {
        try
        {
            item.EnableRaisingEvents = false;
            item.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    public void Watch(string directory, string file, Action onChange, Action onDelete)
    {
        var newWatcher = new FileSystemWatcher(directory, file);
        ActiveWatchers.Add(newWatcher);
        newWatcher.Changed += (s, e) => onChange();
        newWatcher.Deleted += (s, e) =>
        {
            onDelete();
            StopOne(newWatcher);
        };
        newWatcher.EnableRaisingEvents = true;
    }
}

public class ProgramFactory
{
    public static readonly ProgramFactory Instance = new();
    public KnownProgram CreateFor(string filePath, IInteraction? autoStartOnReloadParams = null)
    {
        var programFileInfo = new FileInfo(filePath);
        var programDirInfo = programFileInfo.Directory;
        
        var result = new KnownProgram()
        {
            DirectoryInfo = programDirInfo,
            FileInfo = programFileInfo,
            AutoStartOnReloadParams = autoStartOnReloadParams,
        };

        FileWatcherFactory.Instance.Watch(programDirInfo.FullName, programFileInfo.Name, () => result.Reload(), result.Dispose);

        result.Reload();

        return result;
    }
}
