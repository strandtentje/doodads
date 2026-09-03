#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

[Category("System & IO")]
[Title("Rename a file")]
[Description("""
             Renames a file; sinks new name from sinknewname. will not change path or extension.
             """)]
public class RenameFile : IService
{
    public static event EventHandler<(string oldPath, string newPath)>? FileMoved;
    [NamedSetting("allowmove", """
                               Set this to true to allow the rename to imply a move to a different location
                               """)]
    private readonly UpdatingKeyValue AllowMovingConstant = new UpdatingKeyValue("allowmove");
    [NamedSetting("dontcollide", """
        Set this to true to avoid name collissions by increasing the number prefix.
        """)]
    private readonly UpdatingKeyValue RenumberAgainstCollission = new UpdatingKeyValue("dontcollide");
    private bool CurrentlyAllowsMoving;
    private bool CurrentlyAvoidsCollission;

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
            this.CurrentlyAllowsMoving = allowMoveCandidate == true;
        }
        if ((constants, RenumberAgainstCollission).IsRereadRequired(out bool? avoidCollission))
        {
            this.CurrentlyAvoidsCollission = avoidCollission == true;
        }

        FileInfo? info = null;
        if (interaction.Register is FileInfo registerInfo &&
            registerInfo.Exists)
            info = registerInfo;
        else if (interaction.Register is object pathObject &&
                 pathObject.ToString() is string path &&
                 File.Exists(path))
            info = new FileInfo(path);
        else
        {
            OnElse?.Invoke(this, interaction);
            return;
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
            fullNewPath = ChangeFileNameOnly(info.FullName, cleanedName);
        }

        while (this.CurrentlyAvoidsCollission && File.Exists(fullNewPath))
        {
            var proposedCollidingInfo = new FileInfo(fullNewPath);
            var parentDirectory = proposedCollidingInfo.Directory;
            var collidingName = proposedCollidingInfo.Name;

            var numberPrefix = new string(collidingName.TakeWhile(char.IsNumber).ToArray());
            var remainingName = new string(collidingName.SkipWhile(char.IsNumber).ToArray());

            var currentNumber = 0;
            if (numberPrefix.Length > 0 && int.TryParse(numberPrefix, out var parsedNumber))
                currentNumber = parsedNumber;
            currentNumber++;

            var format = new string('0', Math.Min(3, numberPrefix.Length));
            var newProposedName = $"{currentNumber.ToString(format)}{remainingName}";
            fullNewPath = Path.Combine(parentDirectory.FullName, newProposedName);
        }

        File.Move(info.FullName, fullNewPath);
        FileMoved?.Invoke(this, (info.FullName, fullNewPath));

        OnThen?.Invoke(this, new CommonInteraction(interaction, fullNewPath));
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
