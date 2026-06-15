using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class MpvAppend : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetCustom<MpvPlayer>(out var player) || player == null)
            OnException?.Invoke(this, interaction.AppendRegister("mpvplayer interaction req'd"));
        else if (interaction.Register?.ToString() is not string file || !File.Exists(file))
            OnException?.Invoke(this, interaction.AppendRegister("file not found"));
        else
        {
            player.IsPlayAfterAppend = true;
            player.AppendPlaylist(file);
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}