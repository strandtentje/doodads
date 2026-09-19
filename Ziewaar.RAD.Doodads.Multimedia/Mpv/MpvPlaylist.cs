using Define.Doodads.Expo.Timeline;
using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class MpvPlaylist : MpvService
{
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