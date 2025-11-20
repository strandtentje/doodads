#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

[Category("System & IO")]
[Title("Extra directory information")]
[Description("""
             Determines if a path in register is a dir, and produces dir info.
             """)]
public class DirInfo : IService
{
    [PrimarySetting("Memory name to get info from")]
    private readonly UpdatingPrimaryValue MemoryNameConstant = new();
    [NamedSetting("hidden", "Set this to True, if it is not desired Info filters out hidden files.")]
    private readonly UpdatingKeyValue Hidden = new UpdatingKeyValue("hidden");
    private string? CurrentMemoryName;
    [EventOccasion("Directory information comes out here")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the directory didn't exist.")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when there was no path in memory")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, MemoryNameConstant).IsRereadRequired(out string? memoryName))
            this.CurrentMemoryName = memoryName;
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
        if (infoToWorkWith is DirectoryInfo info)
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
                { "path", info.FullName },
                { "name", info.Name },
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
            payload["count"] = info.GetFiles().Length;
            payload["type"] = "file";
            if (showHidden == true || !info.Attributes.HasFlag(FileAttributes.Hidden) && !info.Name.StartsWith("."))
                OnThen?.Invoke(this, new CommonInteraction(interaction, register: info, memory: payload));
        }
        else
        {
            OnElse?.Invoke(this, interaction);
            return;
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
