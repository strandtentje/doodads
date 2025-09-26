#nullable enable
#pragma warning disable 67
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
[Category("Call Definition Return")]
[Title("Prevent program file reloading")]
[Description("""
             Prevents program file from reloading during runtime, 
             when a change was detected. Useful for hardening prod environments
             that may have strange indexers or wonky samba integrations.
             """)]
public class NeverReload : IService
{
    [PrimarySetting("File path to prevent reloading onto")]
    private readonly UpdatingPrimaryValue FilenameConstant = new();
    private string? NextFilename;
    private string? CurrentFilename;
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no filename was provided.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, FilenameConstant).IsRereadRequired(out object? filename))
            this.NextFilename = filename?.ToString();
        if (string.IsNullOrWhiteSpace(this.NextFilename) || this.NextFilename == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "filename required"));
            return;
        }

        if (this.CurrentFilename == NextFilename)
            return;

        this.NextFilename = (new FileInfo(this.NextFilename).FullName);
        if (this.CurrentFilename != null)
            ResilientCursorTextEmitter.ReloadLocked.Remove(this.CurrentFilename);
        GlobalLog.Instance?.Information("Switching out filename locks from {a} to {b}", CurrentFilename,
            NextFilename);
        this.CurrentFilename = NextFilename;
        ResilientCursorTextEmitter.ReloadLocked.Add(this.CurrentFilename!);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}