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
    private readonly UpdatingKeyValue AllowMovingConstant = new UpdatingKeyValue("allowmove");
    private bool CurrentlyAllowsMoving;

    [EventOccasion("Sink new filename here.")]
    public event CallForInteraction? SinkNewName;

    [EventOccasion("Has renamed file in register")]
    public event CallForInteraction? OnThen;

    [EventOccasion("When no file was found to rename")]
    public event CallForInteraction? OnElse;

    [EventOccasion("Never happens")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, AllowMovingConstant).IsRereadRequired(out bool? allowMoveCandidate))
        {
            this.CurrentlyAllowsMoving = allowMoveCandidate;
        }

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
            var delChars = Path.GetInvalidPathChars();
            string fullNewPath;
            if (CurrentlyAllowsMoving)
            {
                if (requestedName.Any(delChars.Contains))
                {
                    OnException?.Invoke(this, new CommonInteraction(interaction, "new path contains illegal chars"));
                    return;
                }
                fullNewPath = requestedName;
            }
            else
            {
                var cleanedName = string.Concat(requestedName.Where(x => !delChars.Contains(x)));
                fullNewPath = ChangeFileNameOnly(infoToWorkWith.FullName, cleanedName);
            }
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