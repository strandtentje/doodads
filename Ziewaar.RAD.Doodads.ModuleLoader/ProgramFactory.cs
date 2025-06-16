#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class ProgramFactory
{
    public static readonly ProgramFactory Instance = new();
    public ProgramFileLoader CreateFor(string filePath, IInteraction? autoStartOnReloadParams = null)
    {
        var programFileInfo = new FileInfo(filePath);

        var result = new ProgramFileLoader(new ResilientCursorTextEmitter(programFileInfo))
        {
            AutoStartOnReloadParams = autoStartOnReloadParams,
        };

      /*  if (programFileInfo.Directory != null)
            FileWatcherFactory.Instance.Watch(
                programFileInfo.Directory.FullName,
                programFileInfo.Name,
                () => result.Reload(),
                result.Dispose);
        else
            Console.WriteLine($"cannot enable filesystem watcher; no parent directory on {programFileInfo}"); */

        result.Reload();
        return result;
    }
}