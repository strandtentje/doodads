#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.IO;

[Category("System & IO")]
[Title("Rename a file")]
[Description("""
             Renames a file; sinks new name from sinknewname. will not change path or extension.
             """)]
public class RenameFile : IService
{
    public event CallForInteraction? SinkNewName;

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        FileSystemInfo? infoToWorkWith = null;
        if (interaction.Register is FileSystemInfo registerInfo)
        {
            infoToWorkWith = registerInfo;
        }
        else if (interaction.Register is object pathObject &&
            pathObject.ToString() is string path)
        {
            if (File.Exists(path))
                infoToWorkWith = new FileInfo(path);
            else if (Directory.Exists(path))
                infoToWorkWith = new DirectoryInfo(path);
        }
        if (infoToWorkWith is FileSystemInfo info)
        {
            if (!info.Exists)
            {
                OnElse?.Invoke(this, interaction);
            }

            var tsi = new TextSinkingInteraction(interaction);
            SinkNewName?.Invoke(this, tsi);
            var requestedName = tsi.ReadAllText();
            var fullNewPath = ChangeFileNameOnly(infoToWorkWith.FullName, requestedName);
            File.Move(info.FullName, fullNewPath);

            OnThen?.Invoke(this, new CommonInteraction(interaction, fullNewPath));
        }
    }


    public static string ChangeFileNameOnly(string originalFullPath, string newNameMaybeWithPathOrExt)
    {
        // Extract original directory and extension
        string directory = Path.GetDirectoryName(originalFullPath) ?? "";
        string extension = Path.GetExtension(originalFullPath);

        // Get just the file name part of the new name, stripping any path
        string newNameWithMaybeExt = Path.GetFileName(newNameMaybeWithPathOrExt);

        // Strip the extension from the new name (if present)
        string newName = Path.GetFileNameWithoutExtension(newNameWithMaybeExt);

        return Path.Combine(directory, newName + extension);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
