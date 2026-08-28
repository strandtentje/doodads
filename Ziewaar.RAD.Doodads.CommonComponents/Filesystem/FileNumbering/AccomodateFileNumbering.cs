#pragma warning disable 67
using Define.Doodads.Expo.Timeline;
using Ziewaar.RAD.Doodads.CommonComponents.Filesystem.DirectoryOperations;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.FileNumbering;

public class AccomodateFileNumbering : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var workingDirectory = interaction.Register.ToString();
        if (string.IsNullOrWhiteSpace(workingDirectory))
            throw new BasicException("directory required for this");
        if (!Directory.Exists(workingDirectory))
            Directory.CreateDirectory(workingDirectory);

        FileInfo? reorderFile = null;
        int direction = 0;
        if (constants.NamedItems.TryGetValue("up", out var upVarCand))
        {
            if (upVarCand is not string upvar || string.IsNullOrWhiteSpace(upvar) ||
                !interaction.TryFindVariable(upvar, out object? upFileCand) ||
                upFileCand?.ToString() is not string upFile || string.IsNullOrWhiteSpace(upFile) ||
                !File.Exists(upFile))
                throw new BasicException("up was specified, but no file was wound");
            reorderFile = new FileInfo(upFile);
            direction = -1;
        }
        if (constants.NamedItems.TryGetValue("down", out var downVarCand))
        {
            if (downVarCand is not string downvar || string.IsNullOrWhiteSpace(downvar) ||
                !interaction.TryFindVariable(downvar, out object? downFileCand) ||
                downFileCand?.ToString() is not string downFile || string.IsNullOrWhiteSpace(downFile) ||
                !File.Exists(downFile))
                throw new BasicException("down was specified, but no file was wound");
            if (reorderFile != null || direction != 0)
                throw new BasicException("up and down cannot be both specified");
            reorderFile = new FileInfo(downFile);
            direction = 1;
        }

        var dirInfo = new DirectoryInfo(workingDirectory);
        var allFilesNumbered = dirInfo.GetFiles().OrderBy(x => x.Name).Aggregate(
            new RenumberingFileList(reorderFile, direction), (acc, fi) => acc.Append(fi), x => x.Build()).ToArray();
        var renumberedFiles = allFilesNumbered.Where(x => x.IsRenameNeeded).ToArray();

        foreach (var files in renumberedFiles)
            File.Move(files.OldPath, files.IntermediatePath);
        foreach (var files in renumberedFiles)
            File.Move(files.IntermediatePath, files.FinalPath);
    }


}
