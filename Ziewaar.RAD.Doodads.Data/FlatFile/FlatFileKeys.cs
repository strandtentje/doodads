using Define.Doodads.Expo.Timeline;
using System.Collections.Generic;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Data.FlatFile.Support;

namespace Ziewaar.RAD.Doodads.Data.FlatFile;

public class FlatFileKeys : IteratingService
{
    protected override bool RunElse { get; } = false;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (!repeater.TryGetCustom<JsonFile>(out var flatFile) || flatFile == null)
            throw new BasicException("no flat file in scope");
        var items = flatFile.ListAll();
        foreach (var item in items)
            yield return repeater.AppendRegister(item.Key).AppendMemory(new FlatFileMemberMemory(constants.NamedItems, item.Value));
    }
}
