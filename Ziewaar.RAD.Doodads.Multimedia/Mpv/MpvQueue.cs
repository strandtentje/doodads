using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class MpvQueue : IteratingService
{
    protected override bool RunElse { get; } = false;

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (!repeater.TryGetCustom<MpvPlayer>(out var player) || player == null)
            throw new Exception("mpvplayer interaction req'd");
        (string file, string title)[] queue;
        lock (player)
            queue = player.GetQueueFileTitles((int)player.PlaylistPlayingIndex, -1);
        return queue.Select(x => repeater.AppendRegister(x.file).AppendMemory(("title", x.file), ("file", x.title)));
    }
}