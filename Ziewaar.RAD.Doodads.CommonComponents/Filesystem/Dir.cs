#nullable enable
#pragma warning disable 67
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;
[Category("System & IO")]
[Title("Produces a list of directories, given the path currently in the Register.")]
[Description("""
             Dir will query filesystem for the directory path provided in the Register.
             The events are intended for finding out if the Directory exists, what its 
             subdirectories are, and what its files are. Dirs come out at OnThen, Files come out at OnElse
             """)]
public class Dir : IteratingService
{
    [NamedSetting("pattern", "Wildcard-enabled pattern to filter the files to be shown, ie *.txt or cheese.*")]
    private readonly UpdatingKeyValue FileSearchPatternConstant = new("pattern");
    [NamedSetting("filterdirs", "Wildcard-enabled pattern specifically to filter the directories")]
    private readonly UpdatingKeyValue DirSearchPattern = new("filterdirs");
    protected override bool RunElse => true;
    protected override bool OnElseRunningOverride => true;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        DirectoryInfo info = GetDirectoryInfo(constants, repeater, out var dirSearchPattern, out var _);
        DirectoryInfo[] subDirectories = info.GetDirectories(dirSearchPattern, SearchOption.TopDirectoryOnly);
        return subDirectories.Select(repeater.AppendRegister);
    }
    protected override IEnumerable<IInteraction> GetElseItems(StampedMap constants, IInteraction repeater)
    {
        DirectoryInfo info = GetDirectoryInfo(constants, repeater, out var _, out var fileSearchPattern);
        FileInfo[] subFiles = info.GetFiles(fileSearchPattern, SearchOption.TopDirectoryOnly);
        return subFiles.Select(repeater.AppendRegister);
    }
    private DirectoryInfo GetDirectoryInfo(StampedMap constants, IInteraction repeater, out string? dirSearchPattern, out string? fileSearchPattern)
    {
        (constants, FileSearchPatternConstant).IsRereadRequired(() => "*", out  fileSearchPattern);
        (constants, DirSearchPattern).IsRereadRequired(() => "*", out  dirSearchPattern);
        fileSearchPattern ??= "*";
        dirSearchPattern ??= "*";

        var dirPath = repeater.Register.ToString();
        DirectoryInfo info = new DirectoryInfo(dirPath);

        if (!info.Exists)
            throw new Exception("directory not found");

        return info;
    }
}
