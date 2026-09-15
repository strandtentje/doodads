using Define.Doodads.Expo.Timeline;
using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class MpvInstance : BasicService, IDisposable
{
    private readonly Lock InstanceLock = new Lock();
    private bool IsDisposing = true;
    private readonly List<MpvPlayer> ActivePlayers = new List<MpvPlayer>();
    public override event CallForInteraction? OnThen;

    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        lock (InstanceLock)
        {
            if (IsDisposing)
                throw new ObjectDisposedException(nameof(MpvInstance));
            var factory = new MpvPlayerFactory();
            var player = factory.CreatePlayer(IntPtr.Zero, PlaylistLifecycle.PauseAfterEnd);
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