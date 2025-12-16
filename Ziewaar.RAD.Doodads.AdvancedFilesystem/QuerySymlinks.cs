using Microsoft.VisualBasic;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem;
[Category("System & IO")]
[Title("Get list of symlinks in a directory")]
[Description("""
             Provided a directory, gets symlinks in there and stuffs it in memory with from-to pairs.
             """)]
public class QuerySymlinks : IteratingService
{
    protected override bool RunElse => false;

    [EventOccasion("Sink parent dir here")]
    public event CallForInteraction? LinkParentDir;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        var tsi = new TextSinkingInteraction(repeater);
        LinkParentDir?.Invoke(this, tsi);
        var candidateDirPath = tsi.ReadAllText();
        var dirInfo = new DirectoryInfo(candidateDirPath);
        if (!dirInfo.Exists)
            throw new Exception("Existing directory path required for this");

        return SymlinkRepository.Instance.
            ListSymlinks(dirInfo.FullName).
            Where(x => x.IsDirectory == false).
            Select(x => repeater.AppendMemory(("from", x.LinkPath), ("to", x.TargetPath)));
    }

    protected override IEnumerable<IInteraction> GetElseItems(StampedMap constants, IInteraction repeater)
    {
        var tsi = new TextSinkingInteraction(repeater);
        LinkParentDir?.Invoke(this, tsi);
        var candidateDirPath = tsi.ReadAllText();
        var dirInfo = new DirectoryInfo(candidateDirPath);
        if (!dirInfo.Exists)
            throw new Exception("Existing directory path required for this");

        return SymlinkRepository.Instance.
            ListSymlinks(dirInfo.FullName).
            Where(x => x.IsDirectory == true).
            Select(x => repeater.AppendMemory(("from", x.LinkPath), ("to", x.TargetPath)));
    }
}
