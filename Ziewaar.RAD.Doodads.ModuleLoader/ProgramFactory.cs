using System;
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

        programDirWatcher.Changed += (s, e) =>
        {
            result.Reload();
        };
        programDirWatcher.Deleted += (s, e) =>
        {
            result.Dispose();
        };
        programDirWatcher.EnableRaisingEvents = true;

        result.Reload();

        return result;
    }
}
