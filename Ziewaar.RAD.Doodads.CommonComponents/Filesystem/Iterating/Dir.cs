#pragma warning disable 67
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Iterating;

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
        var onlyVisible =
            constants.PrimaryConstant.ToString()?.StartsWith("visible", StringComparison.OrdinalIgnoreCase) == true;
        var subDirectories =
            info.GetDirectories(dirSearchPattern, SearchOption.TopDirectoryOnly)
                .Where(x => !onlyVisible || (!x.Attributes.HasFlag(FileAttributes.Hidden) && !x.Name.StartsWith(".")))
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase);
        return subDirectories.Select(x => repeater.AppendRegister(x).AppendMemory(("onlyname", x.Name)));
    }

    protected override IEnumerable<IInteraction> GetElseItems(StampedMap constants, IInteraction repeater)
    {
        DirectoryInfo info = GetDirectoryInfo(constants, repeater, out var _, out var fileSearchPattern);
        var onlyVisible =
            constants.PrimaryConstant.ToString()?.StartsWith("visible", StringComparison.OrdinalIgnoreCase) == true;
        var subFiles = info.GetFiles(fileSearchPattern, SearchOption.TopDirectoryOnly)
            .Where(x => !onlyVisible || (!x.Attributes.HasFlag(FileAttributes.Hidden) && !x.Name.StartsWith(".")))
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase);
        return subFiles.Select(x => repeater.AppendRegister(x).AppendMemory(("onlyname",x.Name)));
    }

    protected DirectoryInfo GetDirectoryInfo(StampedMap constants, IInteraction repeater, out string? dirSearchPattern,
        out string? fileSearchPattern)
    {
        (constants, FileSearchPatternConstant).IsRereadRequired(() => "*", out fileSearchPattern);
        (constants, DirSearchPattern).IsRereadRequired(() => "*", out dirSearchPattern);
        fileSearchPattern ??= "*";
        dirSearchPattern ??= "*";

        var dirPath = repeater.Register.ToString();
        DirectoryInfo info = new DirectoryInfo(dirPath);

        if (!info.Exists)
            throw new Exception("directory not found");

        return info;
    }
}

public class FindInFiles : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var directory = new DirectoryInfo(DirectoryFromRegister(interaction));
        var extensions = PrimaryParts(constants, []);
        var files = directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Where(x => !x.IsHidden()).Where(x =>
            extensions.Length == 0 || extensions.Contains(x.Extension, StringComparer.OrdinalIgnoreCase));
        var payloads = files.Select(x => new FilesystemInfoPayload(x, null, null));
        RepeatToMemory(constants, interaction, payloads);
    }
}