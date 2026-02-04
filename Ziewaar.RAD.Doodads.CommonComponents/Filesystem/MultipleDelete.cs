#nullable enable
#pragma warning disable 67
using System.Collections;
using System.Data;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

[Category("System & IO")]
[Title("Deletes files by pattern")]
[Description("""
             Provided a dir in the primary const, and a pattern, deletes the files that conform to it.
             """)]
public class MultipleDelete : IService
{
    [PrimarySetting("Path to delete files from")]
    private readonly UpdatingPrimaryValue DirectoryPathConst = new();
    [NamedSetting("pattern", "File pattern like *.bmp to delete")]
    private readonly UpdatingKeyValue PatternConst = new("pattern");
    private string? DirPath;
    private string? Pattern;
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When no pattern was provided.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, DirectoryPathConst).IsRereadRequired(out object? dpc))
            this.DirPath = dpc?.ToString();
        if ((constants, PatternConst).IsRereadRequired(out string? pat))
            this.Pattern = pat;
        if (string.IsNullOrWhiteSpace(DirPath) || string.IsNullOrWhiteSpace(Pattern) || DirPath == null || Pattern == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "pattern required"));
            return;
        }
        var dir = new DirectoryInfo(DirPath);
        var files = dir.GetFiles(Pattern, SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            try
            {
                file.Delete();
            }
            catch (Exception ex)
            {
                GlobalLog.Instance?.Warning(ex, "While deleting file");
            }
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
