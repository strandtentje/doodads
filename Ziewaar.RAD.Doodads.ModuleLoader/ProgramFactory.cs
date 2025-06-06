using System.IO;
using Ziewaar.RAD.Doodads.RKOP;

namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class ProgramFactory
{
    public static readonly ProgramFactory Instance = new();
    public KnownProgram CreateFor(string filePath)
    {
        var programFileInfo = new FileInfo(filePath);
        var programDirInfo = programFileInfo.Directory;
        var programDirWatcher = new FileSystemWatcher(programDirInfo.FullName, programFileInfo.Name);

        var result = new KnownProgram()
        {
            DescriptionRoot = new ServiceDescription<ServiceBuilder>(),
            DirectoryInfo = programDirInfo,
            FileInfo = programFileInfo,
        };

        void Reload()
        {
            var fileInfo = new FileInfo(filePath);
            var cursor = CursorText.Create(fileInfo.Directory, fileInfo.Name, File.ReadAllText(filePath));
            result.DescriptionRoot.UpdateFrom(ref cursor);
        }

        programDirWatcher.Changed += (s, e) =>
        {
            Reload();
        };
        programDirWatcher.Deleted += (s, e) =>
        {
            result.Dispose();
        };

        Reload();

        return result;
    }
}
