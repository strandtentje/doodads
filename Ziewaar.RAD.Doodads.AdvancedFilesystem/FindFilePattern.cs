using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem;

#pragma warning disable 67
public class FindFilePattern : IteratingService
{
    private readonly UpdatingKeyValue Pattern = new UpdatingKeyValue("pattern");
    protected override bool RunElse { get; }

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (repeater.Register?.ToString() is not string dirPath || string.IsNullOrWhiteSpace(dirPath) ||
            !Directory.Exists(dirPath))
            throw new Exception("dir not found");
        if (!constants.NamedItems.TryGetValue("pattern", out var patCand) ||
            patCand?.ToString() is not string patStr || string.IsNullOrWhiteSpace(patStr))
            throw new Exception("pattern required");

        var ef = Directory.EnumerateFiles(dirPath, patStr, new EnumerationOptions()
        {
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.Temporary,
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive,
            MaxRecursionDepth = 64,
            RecurseSubdirectories = true,
        });

        foreach (var f in ef)
        {
            yield return repeater.AppendRegister(f);
        }
    }
}