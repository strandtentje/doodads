#pragma warning disable 67
using Define.Doodads.Expo.Timeline;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.DirectoryOperations;

public static class DeepRenameUtils
{
    public static void FindAndValidateDirectory(IInteraction interaction, string variable, out string directory)
    {
        if (!interaction.TryFindVariable(variable, out string? validPath) ||
            string.IsNullOrWhiteSpace(validPath) || validPath == null ||
            !Directory.Exists(validPath))
            throw new BasicException($"Path `{validPath}` from `{variable}` wasnt in memory or didnt exist");
        directory = validPath;
    }

    public static void FindAndValidateNewName(IInteraction interaction, string variable, out string newName)
    {
        var ipnc = Path.GetInvalidFileNameChars();
        if (!interaction.TryFindVariable(variable, out string? validName) ||
            string.IsNullOrWhiteSpace(validName) || validName == null ||
            validName.Any(x => ipnc.Contains(x)))
            throw new BasicException($"New name `{validName}` from `{variable}` wasnt in memory or contained bad chars");
        newName = validName;
    }

    public static void InspectAndReplaceInFiles(FileInfo[] suspectFiles, string oldName, string newName)
    {
        foreach (var file in suspectFiles)
        {
            var oldText = File.ReadAllText(file.FullName);
            var newText = oldText.Replace(oldName, newName);
            if (newText != oldText)
            {
                GlobalLog.Instance?.Information(
                    "Deep rename found it neccesary to change `{oldName}` into `{newName}` in the file `{file}`",
                    oldName, newName, file.FullName);
                File.WriteAllText(file.FullName, newText);
            }
        }
    }

    public static void InspectAndRenameDirectories(DirectoryInfo parent, string oldName, string newName)
    {
        var collidingDirectories = parent.GetDirectories(oldName, SearchOption.AllDirectories);
        foreach (var collission in collidingDirectories)
        {
            var oldPath = collission.FullName;
            GlobalLog.Instance?.Information(
                "Deep rename found it neccesary to change name of directory `{oldPath}` into `{newName}`",
                oldPath, newName);
            var newPath = Path.Combine(collission.Parent.FullName, newName);
            Directory.Move(oldPath, newPath);
        }
    }

    public static FileInfo[] DetermineFilesToInspect(DirectoryInfo parent, object filesToCheck)
    {

        List<string> namesToInspect = new();
        if (filesToCheck is System.Collections.IEnumerable ie)
            foreach (var item in ie)
                namesToInspect.Add(item.ToString());
        List<FileInfo> infosToInspect = new();        
        foreach (var item in namesToInspect)
            infosToInspect.AddRange(parent.GetFiles(item, SearchOption.AllDirectories));
        FileInfo[] filesToInspect = [.. infosToInspect];
        return filesToInspect;
    }
}
