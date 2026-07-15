#pragma warning disable 67
using Define.Doodads.Expo.Timeline;
using Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Iterating;
using static Ziewaar.RAD.Doodads.CommonComponents.Filesystem.DirectoryOperations.DeepRenameUtils;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.DirectoryOperations;

public class NumberPreservingDeepDirectoryRename : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        if (constants.NamedItems.Count != 1)
            throw new BasicException("Expected exactly one swap definition in constants");

        var swap = constants.NamedItems.First();
        var pathVariable = swap.Key;
        var newNameVariable = swap.Value.ToString();

        FindAndValidateDirectory(interaction, pathVariable, out var oldPath);
        FindAndValidateNewName(interaction, newNameVariable, out var newName);

        var info = new DirectoryInfo(oldPath);
        if (info.TryGetNumberPrefix(out var pfx, out var _))
            newName = $"{pfx}-{newName}";
        var parent = info.Parent;
        var parentPath = parent.FullName;
        var newPath = Path.Combine(parentPath, newName);

        if (Directory.Exists(newPath))
            throw new BasicException("This rename operation would cause a collission");

        var filesToInspect = DetermineFilesToInspect(parent, constants.PrimaryConstant);

        InspectAndReplaceInFiles(filesToInspect, info.Name, newName);
        Directory.Move(oldPath, newPath);
        InspectAndRenameDirectories(parent, info.Name, newName);
    }
}
