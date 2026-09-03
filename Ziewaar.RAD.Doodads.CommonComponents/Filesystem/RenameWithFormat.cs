#pragma warning disable 67
using Define.Doodads.Expo.Timeline;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

[Category("System & IO")]
[Title("Rename a file by format")]
[Description("""
             Works like RenameFile, but does not expose the SinkNewName branch, 
             instead acceping a Format (like the Format service) in the primary constant
             directly.
             """)]
public class RenameWithFormat : BasicService
{
    private StampedMap? Constants;
    private readonly Format FormatService = new Format();
    private readonly RenameFile RenameService = new RenameFile();
    public override event CallForInteraction? OnThen;
    public override event CallForInteraction? OnElse;
    public RenameWithFormat()
    {
        RenameService.SinkNewName += (s, e) => FormatService.Enter(Constants!, e);
        RenameService.OnThen += (s, e) => OnThen?.Invoke(this, e);
        RenameService.OnElse += (s, e) => OnElse?.Invoke(this, e);
    }
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        this.Constants = constants;
        RenameService.Enter(Constants, interaction);
    }
}