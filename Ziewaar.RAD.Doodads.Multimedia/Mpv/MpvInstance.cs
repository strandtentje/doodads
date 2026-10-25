using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia.Mpv;

[Category("Multimedia")]
[Title("Start an instance of MPV")]
[Description("""
             Wakes up MPV and allows it to be manipulated OnThen
             """)]
public class MpvInstance : BasicService, IDisposable
{
    private readonly Lock InstanceLock = new Lock();
    private bool IsDisposing = false;
    private readonly List<MpvPlayer> ActivePlayers = new List<MpvPlayer>();
    [EventOccasion("MPV instance comes out here")]
    public override event CallForInteraction? OnThen;

    [NamedSetting("bg", "Background color as html hex color")]
    private readonly UpdatingKeyValue BackgroundConstant = new UpdatingKeyValue("bg");
    
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        lock (InstanceLock)
        {
            if (IsDisposing)
                throw new ObjectDisposedException(nameof(MpvInstance));
            var bgColor = Named(constants, "bg") ?? "#000000";
            var factory = new MpvPlayerFactory();
            var player = factory.CreatePlayer(IntPtr.Zero, PlaylistLifecycle.PauseAfterEnd, forceWindow: true, backgroundColor: bgColor);
            ActivePlayers.Add(player);
            OnThen?.Invoke(this, interaction.AppendCustom(player));
        }
    }

    public void Dispose()
    {
        lock (InstanceLock)
        {
            if (IsDisposing) return;
            IsDisposing = true;
            foreach (var activePlayer in ActivePlayers)
            {
                activePlayer.Dispose();
            }
        }
    }
}