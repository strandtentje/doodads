using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class MpvOperate : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetCustom<MpvPlayer>(out var player) || player == null)
            OnException?.Invoke(this, interaction.AppendRegister("mpvplayer interaction req'd"));
        else if (interaction.Register?.ToString() is not string opString ||
                 !Enum.TryParse<MpvOperations>(opString, ignoreCase: true, out var operation))
            OnException?.Invoke(this, interaction.AppendRegister("missing or invalid operation"));
        else
        {
            lock (player)
            {
                switch (operation)
                {
                    case MpvOperations.Pause:
                        player.IsPause = false;
                        break;
                    case MpvOperations.Unpause:
                        player.IsPause = true;
                        break;
                    case MpvOperations.Previous:
                        player.Previous();
                        break;
                    case MpvOperations.Next:
                        player.Next();
                        break;
                    case MpvOperations.ReplayCurrent:
                        var ix = Math.Max((int)player.PlaylistPlayingIndex, (int)player.PlaylistIndex);
                        player.GoToPlaylistItem(ix);
                        break;
                    case MpvOperations.SeekBack:
                        player.SeekRelativePercent(-10);
                        break;
                    case MpvOperations.SeekForward:
                        player.SeekRelativePercent(10);
                        break;
                }
            }
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}