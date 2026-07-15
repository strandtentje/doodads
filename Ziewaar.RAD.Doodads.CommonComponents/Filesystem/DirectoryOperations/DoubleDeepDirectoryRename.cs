#pragma warning disable 67
using Define.Doodads.Expo.Timeline;
using static Ziewaar.RAD.Doodads.CommonComponents.Filesystem.DirectoryOperations.DeepRenameUtils;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.DirectoryOperations;

public class DoubleDeepDirectoryRename : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        if (constants.NamedItems.Count != 2)
            throw new BasicException("Expected exactly two swap definitions in constants");

        var firstSwap = constants.NamedItems.First();
        var secondSwap = constants.NamedItems.Last();

        var firstCurrentPathVariable = firstSwap.Key;
        var firstNewNameVariable = firstSwap.Value.ToString();

        var secondCurrentPathVariable = secondSwap.Key;
        var secondNewNameVariable = secondSwap.Value.ToString();

        FindAndValidateDirectory(interaction, firstCurrentPathVariable, out var oldPathA);
        FindAndValidateDirectory(interaction, secondCurrentPathVariable, out var oldPathB);
        FindAndValidateNewName(interaction, firstNewNameVariable, out var newNameA);
        FindAndValidateNewName(interaction, secondNewNameVariable, out var newNameB);

        var intermediaryName = new string([.. Guid.NewGuid().ToString().Where(char.IsLetterOrDigit)]);

        var oldInfoA = new DirectoryInfo(oldPathA);
        var oldInfoB = new DirectoryInfo(oldPathB);

        if (oldInfoA.Parent.FullName != oldInfoB.Parent.FullName)
            throw new BasicException(
                $"Directory `{oldPathA}` from `{firstCurrentPathVariable}` and Directory `{oldPathB}` from `{secondCurrentPathVariable}` dont have the same parent.");

        var parent = oldInfoA.Parent;
        var parentPath = parent.FullName;

        var intermediaryNewPath = Path.Combine(parentPath, intermediaryName);
        var newPathA = Path.Combine(parentPath, newNameA);
        var newPathB = Path.Combine(parentPath, newNameB);

        var filesToInspect = DetermineFilesToInspect(parent, constants.PrimaryConstant);

        InspectAndReplaceInFiles(filesToInspect, oldInfoA.Name, intermediaryName);
        InspectAndReplaceInFiles(filesToInspect, oldInfoB.Name, newNameB);
        InspectAndReplaceInFiles(filesToInspect, intermediaryName, newNameA);

        Directory.Move(oldPathA, intermediaryNewPath);
        InspectAndRenameDirectories(parent, oldInfoA.Name, intermediaryName);
            
        Directory.Move(oldPathB, newPathB);
        InspectAndRenameDirectories(parent, oldInfoB.Name, newNameB);

        Directory.Move(intermediaryNewPath, newPathA);
        InspectAndRenameDirectories(parent, intermediaryName, newNameA);
    }
}
