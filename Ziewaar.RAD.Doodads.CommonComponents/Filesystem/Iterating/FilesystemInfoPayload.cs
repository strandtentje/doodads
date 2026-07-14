#pragma warning disable 67
using System.Collections;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Iterating;

public class FilesystemInfoPayload : IReadOnlyDictionary<string, object>
{
    private const string
        KEY_VISIBILITY = "visibility", KEY_NUMBERPREFIX = "numberprefix", KEY_AFTERNUMBER = "afternumber",
        KEY_NEXTNUMBER = "nextnumberprefix", KEY_PREVNUMBER = "previousnumberprefix", KEY_PATH = "path",
        KEY_NEXTAFTERNUMBER = "nextafternumber", KEY_PREVAFTERNUMBER = "prevafternumber",
        KEY_NAME = "name", KEY_LAST_WRITE_TIME = "write", KEY_LAST_READ_TIME = "read", KEY_URLSAFE_PATH = "safepath",
        KEY_EXTENSION = "extension", KEY_CLEAN_EXTENSION = "cleanext", KEY_CLEAN_NAME = "cleanname", KEY_SIZE = "size",
        KEY_CLEAN_SIZE = "cleansize", KEY_DIR_OR_FILE = "type", KEY_FILE_COUNT = "count";
    private readonly FileInfo? FileInfo;
    private readonly DirectoryInfo? DirectoryInfo;
    private readonly FileSystemInfo FilesystemInfo;
    private readonly string? PathVariable, NameVariable;
    private string? NextCalculatedNumber = null;
    private string? PrevCalculatedNumber = null;
    private string? NextNumberlessFile;
    private string? PrevNumberlessFile;

    private string NumberPrefix => field ??= this.FilesystemInfo.GetNumberPrefix();
    private string AfterNumberPrefix => field ??= this.FilesystemInfo.GetAfterNumberPrefix();
    private string SafePath => string.Concat(FilesystemInfo.FullName.Select(x =>
    {
        if (char.IsLetterOrDigit(x))
            return x.ToString();
        else
            return Uri.HexEscape(x);
    }));

    public IEnumerable<string> Keys { get; }
    public IEnumerable<object> Values => Keys.Select(x => this[x]);
    public int Count => Keys.Count();

    public FilesystemInfoPayload(FileSystemInfo filesystemInfo, string? pathVariable, string? nameVariable)
    {
        this.FilesystemInfo = filesystemInfo;
        this.PathVariable = pathVariable;
        this.NameVariable = nameVariable;
        if (filesystemInfo is FileInfo fi)
        {
            this.FileInfo = fi;
            Keys = [
                KEY_VISIBILITY, KEY_NUMBERPREFIX, KEY_AFTERNUMBER,
                    KEY_NEXTNUMBER, KEY_PREVNUMBER, KEY_NEXTAFTERNUMBER, KEY_PREVAFTERNUMBER,
                    pathVariable ?? KEY_PATH, nameVariable ?? KEY_NAME,
                    KEY_LAST_WRITE_TIME, KEY_LAST_READ_TIME, KEY_URLSAFE_PATH, KEY_EXTENSION,
                    KEY_CLEAN_EXTENSION, KEY_CLEAN_NAME, KEY_SIZE, KEY_CLEAN_SIZE, KEY_DIR_OR_FILE,
                ];
        }
        else if (filesystemInfo is DirectoryInfo di)
        {
            this.DirectoryInfo = di;
            Keys = [
                KEY_VISIBILITY, KEY_NUMBERPREFIX, KEY_AFTERNUMBER,
                    KEY_NEXTNUMBER, KEY_PREVNUMBER, KEY_NEXTAFTERNUMBER, KEY_PREVAFTERNUMBER,
                    pathVariable ?? KEY_PATH, nameVariable ?? KEY_NAME,
                    KEY_LAST_WRITE_TIME, KEY_LAST_READ_TIME, KEY_URLSAFE_PATH, KEY_FILE_COUNT, KEY_DIR_OR_FILE,
                ];
        }
        else
        {
            throw new ArgumentException("Expected fileinfo or directoryinfo", nameof(filesystemInfo));
        }
    }

    public object this[string key] =>
        TryGetValue(key, out var val) ? val : throw new KeyNotFoundException();

    public bool ContainsKey(string key) => Keys.Contains(key);

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        foreach (var item in Keys)
            yield return new KeyValuePair<string, object>(item, this[item]);
    }

    public bool TryGetValue(string key, out object value)
    {
        if (key == (this.PathVariable ?? KEY_PATH))
        {
            value = this.FilesystemInfo.FullName;
            return true;
        }
        if (key == (this.NameVariable ?? KEY_NAME))
        {
            value = this.FilesystemInfo.Name;
            return true;
        }
        switch (key)
        {
            case KEY_VISIBILITY:
                value = this.FilesystemInfo.IsHidden() ? "visible" : "hidden";
                return true;
            case KEY_NUMBERPREFIX:
                value = NumberPrefix;
                return true;
            case KEY_AFTERNUMBER:
                value = AfterNumberPrefix;
                return true;
            case KEY_NEXTNUMBER:
                if (this.NextCalculatedNumber is string ncn)
                    value = ncn;
                else
                {
                    this.FilesystemInfo.GetNextSplittable(out var pfx, out var rem);
                    value = this.NextCalculatedNumber = pfx;
                    this.NextNumberlessFile = rem;
                }

                return true;

            case KEY_PREVNUMBER:
                if (this.PrevCalculatedNumber is string pcn)
                    value = pcn;
                else
                {
                    this.FilesystemInfo.GetPrevSplittable(out var pfx, out var rem);
                    value = this.PrevCalculatedNumber = pfx;
                    this.PrevNumberlessFile = rem;
                }

                return true;

            case KEY_NEXTAFTERNUMBER:
                if (this.NextNumberlessFile is string nnlf)
                    value = nnlf;
                else
                {
                    this.FilesystemInfo.GetNextSplittable(out var pfx, out var rem);
                    this.NextCalculatedNumber = pfx;
                    value = this.NextNumberlessFile = rem;
                }

                return true;

            case KEY_PREVAFTERNUMBER:
                if (this.PrevNumberlessFile is string pnlf)
                    value = pnlf;
                else
                {
                    this.FilesystemInfo.GetPrevSplittable(out var pfx, out var rem);
                    this.PrevCalculatedNumber = pfx;
                    value = this.PrevNumberlessFile = rem;
                }

                return true;

            case KEY_LAST_WRITE_TIME:
                value = this.FilesystemInfo.LastWriteTimeUtc;
                return true;
            case KEY_LAST_READ_TIME:
                value = this.FilesystemInfo.LastAccessTimeUtc;
                return true;
            case KEY_URLSAFE_PATH:
                value = this.SafePath;
                return true;
            case KEY_DIR_OR_FILE when this.DirectoryInfo is { }:
                value = "dir";
                return true;
            case KEY_DIR_OR_FILE when this.FileInfo is { }:
                value = "file";
                return true;
            case KEY_FILE_COUNT when this.DirectoryInfo is { }:
                value = this.DirectoryInfo.GetFiles().Length;
                return true;
            case KEY_EXTENSION when this.FileInfo is { }:
                value = this.FileInfo.Extension;
                return true;
            case KEY_CLEAN_EXTENSION when this.FileInfo is { }:
                value = this.FileInfo.Extension.TrimStart('.').ToLower();
                return true;
            case KEY_CLEAN_NAME when this.FileInfo is { }:
                value = Path.GetFileNameWithoutExtension(FileInfo.FullName);
                return true;
            case KEY_SIZE when this.FileInfo is { }:
                value = this.FileInfo.Length;
                return true;
            case KEY_CLEAN_SIZE when this.FileInfo is { }:
                value = ByteSizeFormatter.ToHumanReadable(this.FileInfo.Length);
                return true;
            default:
                value = "";
                return false;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}