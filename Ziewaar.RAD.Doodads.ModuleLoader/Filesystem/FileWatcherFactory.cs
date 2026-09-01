#nullable enable
using Ejije.Logging;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;
public class FileWatcherFactory : IDisposable
{
    private static FileWatcherFactory? backing;
    public static FileWatcherFactory Instance
    {
        get => backing ??= new();
        private set => backing = value;
    }
    private readonly List<FileSystemWatcher> ActiveWatchers = new();

    public void Dispose()
    {
        backing = null;
        foreach (var item in ActiveWatchers)
        {
            StopOne(item);
        }
    }
    private static readonly Log DisposeFail = Log.Warn("Disposal of Filewatcher failed due to {exception}");
    private static void StopOne(FileSystemWatcher item)
    {
        try
        {
            item.EnableRaisingEvents = false;
            item.Dispose();
        }
        catch (Exception ex)
        {
            Log.Post(DisposeFail, ex);
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