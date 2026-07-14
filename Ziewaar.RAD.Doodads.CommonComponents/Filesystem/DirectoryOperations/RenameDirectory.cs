#pragma warning disable 67
using Serilog.Core;
using System.Runtime.CompilerServices;
using Ziewaar.RAD.Doodads.CommonComponents.Generate;
using Ziewaar.RAD.Doodads.CommonComponents.TextTests;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.DirectoryOperations;

[Category("System & IO")]
[Title("Rename a directory")]
[Description("""
             Renames a directory; sinks new name from sinknewname. will not change path or extension.
             """)]
public class RenameDirectory : IService
{
    [NamedSetting("allowmove", """
                               Set this to true to allow the rename to imply a move to a different location
                               """)]
    private readonly UpdatingKeyValue AllowMovingConstant = new UpdatingKeyValue("allowmove");
    private bool CurrentlyAllowsMoving;

    [EventOccasion("Sink new dir name here.")]
    public event CallForInteraction? SinkNewName;

    [EventOccasion("Has renamed dir in register")]
    public event CallForInteraction? OnThen;

    [EventOccasion("When no dir was found to rename")]
    public event CallForInteraction? OnElse;

    [EventOccasion("Never happens")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, AllowMovingConstant).IsRereadRequired(out bool? allowMoveCandidate))
        {
            this.CurrentlyAllowsMoving = allowMoveCandidate == true;
        }

        DirectoryInfo? info = null;
        if (interaction.Register is DirectoryInfo registerInfo &&
            registerInfo.Exists)
            info = registerInfo;
        else if (interaction.Register is object pathObject &&
                 pathObject.ToString() is string path &&
                 Directory.Exists(path))
            info = new DirectoryInfo(path);
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

            var parentName = info.Parent.FullName;

            fullNewPath = Path.Combine(parentName, cleanedName);
        }
        Directory.Move(info.FullName, fullNewPath);

        OnThen?.Invoke(this, new CommonInteraction(interaction, fullNewPath));
    }


    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
