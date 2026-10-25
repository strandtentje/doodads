using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Multimedia.Mpv.Interaction;

namespace Ziewaar.RAD.Doodads.Multimedia.Mpv;

[Category("Multimedia")]
[Title("Iterate through mpv playlist")]
[Description("""
             Requires MpvInstance to have happened before, iterates through playlist items.
             """)]
public class MpvPlaylist : MpvService
{
    [EventOccasion("""
                   Iterates like any other iterator here, requiring [Continue]. The following information is 
                   available in memory: ordinal, file, title, source, playing
                   """)]
    public override event CallForInteraction? OnThen;
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForNullOrEmpty(Primary(constants), "repeat name required", out var repeatName);
        var ri = new RepeatInteraction(repeatName, interaction, CancellationToken.None);
        foreach (var member in player.EnumeratePlaylistMembers())
        {
            OnThen?.Invoke(this, new PlaylistMemberInteraction(ri, member));
        }
    }
}