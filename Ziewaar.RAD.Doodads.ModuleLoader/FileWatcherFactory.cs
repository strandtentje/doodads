#nullable enable
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