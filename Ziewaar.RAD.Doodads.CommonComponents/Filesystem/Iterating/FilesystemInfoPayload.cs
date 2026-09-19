#pragma warning disable 67
using System.Collections;
using System.ComponentModel.Design;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Iterating;

[Flags]
public enum FilesystemInfoPayloadKeys : uint
{
    Visibility = 1 << 0,
    NumberPrefix = 1 << 1,
    AfterNumber = 1 << 2,
    NextNumberPrefix = 1 << 3,
    PreviousNumberPrefix = 1 << 4,
    Path = 1 << 5,
    FreeNumber = 1 << 6,
    EmojiFileType = 1 << 7,
    NextAfterNumber = 1 << 8,
    PrevAfterNumber = 1 << 9,
    NextNumberedPath = 1 << 10,
    PrevNumberedPath = 1 << 11,
    Name = 1 << 12,
    Write = 1 << 13,
    Read = 1 << 14,
    SafePath = 1 << 15,
    Extension = 1 << 16,
    CleanExt = 1 << 17,
    CleanName = 1 << 18,
    Size = 1 << 19,
    CleanSize = 1 << 20,
    Type = 1 << 21,
    Count = 1 << 22,
    AllCount = 1 << 23,

    ForFile = Visibility | NumberPrefix | AfterNumber | NextNumberPrefix | PreviousNumberPrefix | NextAfterNumber |
              PrevAfterNumber | FreeNumber | EmojiFileType | NextNumberedPath | PrevNumberedPath | Write |
              Read | SafePath | Extension | CleanExt | CleanName | Size | CleanSize | Type,

    ForDirectory = Visibility | NumberPrefix | AfterNumber | NextNumberPrefix | PreviousNumberPrefix | NextAfterNumber |
                   PrevAfterNumber | FreeNumber | NextNumberedPath | PrevNumberedPath | Write | Read |
                   SafePath | Type | Count | AllCount,

    ForPathAndName = Path | Name,
}

public class FilesystemInfoPayload(
    FileSystemInfo filesystemInfo,
    string? optionalPathVariable,
    string? optionalNameVariable) : IReadOnlyDictionary<string, object>
{
    private FileInfo? FileInfo => filesystemInfo as FileInfo;
    private DirectoryInfo? DirectoryInfo => filesystemInfo as DirectoryInfo;
    private string PathVariable => field ??=
        (optionalPathVariable ?? Enum.GetName(typeof(FilesystemInfoPayloadKeys), FilesystemInfoPayloadKeys.Path))!;
    private string NameVariable => field ??=
        (optionalNameVariable ?? Enum.GetName(typeof(FilesystemInfoPayloadKeys), FilesystemInfoPayloadKeys.Name))!;
    
    private static readonly IReadOnlyDictionary<string, FilesystemInfoPayloadKeys> KeyLUT =
        Enum.GetValues(typeof(FilesystemInfoPayloadKeys)).Cast<FilesystemInfoPayloadKeys>()
            .ToDictionary(x => Enum.GetName(typeof(FilesystemInfoPayloadKeys), x)!.ToLower(), x => x,
                StringComparer.Ordinal);

    private string? NextCalculatedNumber = null;
    private string? PrevCalculatedNumber = null;
    private string? NextNumberlessFile;
    private string? NextPath;
    private string? PrevNumberlessFile;
    private object? PrevPath;
    private string? CachedFreeNumber;

    private string NumberPrefix => field ??= filesystemInfo.GetNumberPrefix();
    private string AfterNumberPrefix => field ??= filesystemInfo.GetAfterNumberPrefix();

    private string SafePath => field ??=
        string.Concat(filesystemInfo.FullName.Select(x => char.IsLetterOrDigit(x) ? x.ToString() : Uri.HexEscape(x)));

    private IEnumerable<string> GetNamesForMask(FilesystemInfoPayloadKeys mask) => Enum
        .GetValues(typeof(FilesystemInfoPayloadKeys))
        .Cast<FilesystemInfoPayloadKeys>()
        .Where(x => mask.HasFlag(x) && x != mask)
        .Select(x => Enum.GetName(typeof(FilesystemInfoPayloadKeys), x));

    private IEnumerable<string> AppropariateBaseKeys => field ??= filesystemInfo switch
    {
        System.IO.DirectoryInfo => GetNamesForMask(FilesystemInfoPayloadKeys.ForDirectory),
        System.IO.FileInfo => GetNamesForMask(FilesystemInfoPayloadKeys.ForFile),
        _ => throw new InvalidOperationException("unsupported fs entity type"),
    };

    public IEnumerable<string> Keys => field ??= AppropariateBaseKeys
        .Append(NameVariable)
        .Append(PathVariable).Select(x => x!.ToLower());

    public IEnumerable<object> Values => Keys.Select(x => this[x]);
    public int Count => Keys.Count();

    public object this[string key] =>
        TryGetValue(key, out var val) ? val : throw new KeyNotFoundException();

    public bool ContainsKey(string key) => Keys.Contains(key);

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
        Keys.Select(item => new KeyValuePair<string, object>(item, this[item])).GetEnumerator();

    public bool TryGetValue(string key, out object value)
    {
        if (string.Equals(key, this.PathVariable, StringComparison.Ordinal))
        {
            value = filesystemInfo.FullName;
            return true;
        }

        if (string.Equals(key, this.NameVariable, StringComparison.Ordinal))
        {
            value = filesystemInfo.Name;
            return true;
        }

        if (!KeyLUT.TryGetValue(key, out var selection))
        {
            value = string.Empty;
            return false;
        }

        switch (selection)
        {
            case FilesystemInfoPayloadKeys.Visibility:
                value = filesystemInfo.IsHidden() ? "visible" : "hidden";
                return true;
            case FilesystemInfoPayloadKeys.NumberPrefix:
                value = NumberPrefix;
                return true;
            case FilesystemInfoPayloadKeys.AfterNumber:
                value = AfterNumberPrefix;
                return true;
            case FilesystemInfoPayloadKeys.NextNumberPrefix:
                if (this.NextCalculatedNumber is string ncn)
                    value = ncn;
                else
                {
                    filesystemInfo.GetNextSplittable(out var pfx, out var rem, out var path);
                    value = this.NextCalculatedNumber = pfx;
                    this.NextNumberlessFile = rem;
                    this.NextPath = path;
                }

                return true;
            case FilesystemInfoPayloadKeys.PreviousNumberPrefix:
                if (this.PrevCalculatedNumber is string pcn)
                    value = pcn;
                else
                {
                    filesystemInfo.GetPrevSplittable(out var pfx, out var rem, out var path);
                    value = this.PrevCalculatedNumber = pfx;
                    this.PrevNumberlessFile = rem;
                    this.PrevPath = path;
                }

                return true;

            case FilesystemInfoPayloadKeys.NextAfterNumber:
                if (this.NextNumberlessFile is string nnlf)
                    value = nnlf;
                else
                {
                    filesystemInfo.GetNextSplittable(out var pfx, out var rem, out var path);
                    this.NextCalculatedNumber = pfx;
                    this.NextPath = path;
                    value = this.NextNumberlessFile = rem;
                }

                return true;

            case FilesystemInfoPayloadKeys.PrevAfterNumber:
                if (this.PrevNumberlessFile is string pnlf)
                    value = pnlf;
                else
                {
                    filesystemInfo.GetPrevSplittable(out var pfx, out var rem, out var path);
                    this.PrevCalculatedNumber = pfx;
                    this.PrevPath = path;
                    value = this.PrevNumberlessFile = rem;
                }

                return true;

            case FilesystemInfoPayloadKeys.NextNumberedPath:
                if (this.NextPath is string np)
                    value = np;
                else
                {
                    filesystemInfo.GetNextSplittable(out var pfx, out var rem, out var path);
                    this.NextCalculatedNumber = pfx;
                    this.NextNumberlessFile = rem;
                    value = this.NextPath = path;
                }

                return true;

            case FilesystemInfoPayloadKeys.PrevNumberedPath:
                if (this.PrevPath is string pp)
                    value = pp;
                else
                {
                    filesystemInfo.GetPrevSplittable(out var pfx, out var rem, out var path);
                    this.PrevCalculatedNumber = pfx;
                    this.PrevNumberlessFile = rem;
                    value = this.PrevPath = path;
                }

                return true;
            case FilesystemInfoPayloadKeys.EmojiFileType
                when EmojiFileIcons.Mapping.TryGetValue(FileInfo.Extension, out var emoji):
                value = emoji;
                return true;
            case FilesystemInfoPayloadKeys.EmojiFileType:
                value = EmojiFileIcons.DEFAULT_FILE;
                return true;
            case FilesystemInfoPayloadKeys.Write:
                value = filesystemInfo.LastWriteTimeUtc;
                return true;
            case FilesystemInfoPayloadKeys.Read:
                value = filesystemInfo.LastAccessTimeUtc;
                return true;
            case FilesystemInfoPayloadKeys.SafePath:
                value = this.SafePath;
                return true;
            case FilesystemInfoPayloadKeys.Type when filesystemInfo is DirectoryInfo:
                value = "dir";
                return true;
            case FilesystemInfoPayloadKeys.Type when filesystemInfo is FileInfo:
                value = "file";
                return true;
            case FilesystemInfoPayloadKeys.FreeNumber when this.DirectoryInfo is { }:
                if (CachedFreeNumber is string cfn)
                {
                    value = cfn;
                    return true;
                }

                var lastDirectories = this.DirectoryInfo.GetDirectories().OrderByDescending(x => x.Name);
                foreach (var item in lastDirectories)
                {
                    if (item.TryGetNumberPrefix(out var pfx, out var _))
                    {
                        if (pfx.Length > 0 && int.TryParse(pfx, out var pfxInt))
                        {
                            if ((pfxInt + 100) < 999)
                            {
                                value = CachedFreeNumber = $"{(pfxInt + 100):000}";
                            }
                            else if ((pfxInt + 50) < 999)
                            {
                                value = CachedFreeNumber = $"{(pfxInt + 50):000}";
                            }
                            else if ((pfxInt + 25) < 999)
                            {
                                value = CachedFreeNumber = $"{(pfxInt + 25):000}";
                            }
                            else if ((pfxInt + 10) < 999)
                            {
                                value = CachedFreeNumber = $"{(pfxInt + 10):000}";
                            }
                            else if ((pfxInt + 5) < 999)
                            {
                                value = CachedFreeNumber = $"{(pfxInt + 5):000}";
                            }
                            else if ((pfxInt + 1) < 999)
                            {
                                value = CachedFreeNumber = $"{(pfxInt + 1):000}";
                            }
                            else
                            {
                                value = CachedFreeNumber = "";
                            }

                            return true;
                        }
                    }
                }

                value = CachedFreeNumber = "100";
                return true;
            case FilesystemInfoPayloadKeys.Count when filesystemInfo is DirectoryInfo di:
                value = di.EnumerateFiles().Count();
                return true;
            case FilesystemInfoPayloadKeys.AllCount when filesystemInfo is DirectoryInfo dia:
                value = dia.EnumerateFileSystemInfos().Count();
                return true;
            case FilesystemInfoPayloadKeys.Extension when filesystemInfo is FileInfo fiExt:
                value = fiExt.Extension;
                return true;
            case FilesystemInfoPayloadKeys.CleanExt when filesystemInfo is FileInfo fiCExt:
                value = fiCExt.Extension.TrimStart('.').ToLower();
                return true;
            case FilesystemInfoPayloadKeys.CleanName when filesystemInfo is FileInfo fiC :
                value = Path.GetFileNameWithoutExtension(fiC.FullName);
                return true;
            case FilesystemInfoPayloadKeys.Size when filesystemInfo is FileInfo fiS:
                value = fiS.Length;
                return true;
            case FilesystemInfoPayloadKeys.CleanSize when filesystemInfo is FileInfo fiCs:
                value = ByteSizeFormatter.ToHumanReadable(fiCs.Length);
                return true;
            default:
                value = "";
                return false;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}