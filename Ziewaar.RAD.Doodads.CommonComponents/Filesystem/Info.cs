#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

[Category("System & IO")]
[Title("Extra file and directory information reader")]
[Description("""
             Commonly used in conjunction with Dir. Provided a path in the register,
             Info reads the full path, just the name and some dates.
             It will also query the hidden-attribute and reject files and folders starting with a period '.'
             """)]
public class Info : IService
{
    [PrimarySetting("Memory name to get info from")]
    private readonly UpdatingPrimaryValue MemoryNameConstant = new();
    [NamedSetting("hidden", "Set this to True, if it is not desired Info filters out hidden files.")]
    private readonly UpdatingKeyValue Hidden = new UpdatingKeyValue("hidden");
    private string? CurrentMemoryName;
    private string[]? OutputMemoryNames;
    private string? NameVariable, PathVariable;

    [EventOccasion("""
                   Occurs when the file or directory existed, and information is available. Information is not 
                   put in the Register, but into Memory directly, such that: 
                    - `path` contains the full path to this filesystem entry
                    - `name` contains only the name of this entry itself. this may include extensions
                    - `write` contains the last time the file was written to
                    - `read` contains the last time the file was read from
                   """)]
    public event CallForInteraction? OnThen;
    [EventOccasion("Occurs when the file or directory did not exist. Register and Memory are not altered.")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Occurs when the contents of the Register was not something that could be queried in the filesystem.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, MemoryNameConstant).IsRereadRequired(out string? memoryName))
            this.CurrentMemoryName = memoryName;
        if (this.CurrentMemoryName == null &&
            ((constants, MemoryNameConstant).IsRereadRequired(out object[]? outputNames) ||
            outputNames != null))
        {
            this.OutputMemoryNames = outputNames?.OfType<string>().ToArray();
            NameVariable = this.OutputMemoryNames?.FirstOrDefault(x => x.EndsWith("name", StringComparison.OrdinalIgnoreCase)) ?? "name";
            PathVariable = this.OutputMemoryNames?.FirstOrDefault(x => x.EndsWith("path", StringComparison.OrdinalIgnoreCase)) ?? "path";
        }


        (constants, Hidden).IsRereadRequired(() => false, out bool? showHidden);
        FileSystemInfo? infoToWorkWith = null;
        var requestedFile = interaction.Register;
        if (this.CurrentMemoryName != null)
        {
            if (!interaction.TryFindVariable(this.CurrentMemoryName, out object? memoryValue)
                || memoryValue == null)
            {
                OnException?.Invoke(this, interaction.AppendRegister("memory name not found"));
                return;
            }
            else
            {
                requestedFile = memoryValue;
            }
        }
        if (requestedFile is FileSystemInfo registerInfo)
        {
            infoToWorkWith = registerInfo;
        }
        else if (requestedFile is object pathObject &&
            pathObject.ToString() is string path)
        {
            if (File.Exists(path))
                infoToWorkWith = new FileInfo(path);
            else if (Directory.Exists(path))
                infoToWorkWith = new DirectoryInfo(path);
            else
            {
                OnElse?.Invoke(this, interaction);
                return;
            }
        }
        if (infoToWorkWith is FileSystemInfo info)
        {
            if (!info.Exists)
            {
                OnElse?.Invoke(this, interaction);
                return;
            }
            var numberPrefix = info.Name.TakeWhile(char.IsDigit);
            var afterNumberPrefix = info.Name.SkipWhile(char.IsDigit);

            var payload = new SortedList<string, object>()
            {
                { "visibility", info.IsHidden() ? "visible" : "hidden" },
                { "numberprefix", numberPrefix },
                { "afternumber", afterNumberPrefix },
                { PathVariable ?? "path", info.FullName },
                { NameVariable ?? "name", info.Name },
                { "write", info.LastWriteTimeUtc },
                { "read", info.LastAccessTimeUtc },
            };
            payload["safepath"] = string.Concat(info.FullName.Select(x =>
            {
                if (char.IsLetterOrDigit(x))
                    return x.ToString();
                else
                    return Uri.HexEscape(x);
            }));
            if (info is FileInfo fileInfo)
            {
                payload["extension"] = fileInfo.Extension;
                payload["cleanext"] = fileInfo.Extension.TrimStart('.').ToLower();
                payload["cleanname"] = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                payload["size"] = fileInfo.Length;
                payload["cleansize"] = ByteSizeFormatter.ToHumanReadable(fileInfo.Length);
                payload["type"] = "dir";
            }
            else if (info is DirectoryInfo directoryInfo)
            {
                payload["count"] = directoryInfo.GetFiles().Length;
                payload["type"] = "file";
            }
            if (showHidden == true || !info.Attributes.HasFlag(FileAttributes.Hidden) && !info.Name.StartsWith("."))
                OnThen?.Invoke(this, new CommonInteraction(interaction, register: info, memory: payload));
        }
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "this is not a file or directory"));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
