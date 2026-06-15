using System.Globalization;
using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class MpvHistory : IteratingService
{
    private readonly UpdatingKeyValue ResultCapConst = new("resultcap");
    protected override bool RunElse { get; } = false;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (!repeater.TryGetCustom<MpvPlayer>(out var player) || player == null)
            throw new Exception("mpvplayer interaction req'd");

        var cap = -1;
        if (constants.NamedItems.TryGetValue("resultcap", out var value) && value.ToString() is string capStr &&
            int.TryParse(capStr, NumberStyles.Any, CultureInfo.InvariantCulture, out int capNum))
            cap = capNum;
        
        (string file, string title)[] queue;
        lock (player)
            queue = player.GetHistoryFileTitles((int)player.PlaylistPlayingIndex, cap);
        return queue.Select(x => repeater.AppendRegister(x.file).AppendMemory(("title", x.file), ("file", x.title)));
    }
}