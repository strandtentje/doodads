#nullable enable
#pragma warning disable 67

using System.Text.RegularExpressions;

namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;
[Category("Scheduling & Flow")]
[Title("Blocks from here on out to prevent premature finishing of the execution")]
[Description("""
             Will not return control to scope above until FixedDaemonControl is used for 
             the current name. Useful for daemonizing otherwise transient services.
             """)]
public class FixedDaemonControl : IService
{
    [PrimarySetting("Instruction (stop/restart), then pattern to match repeat name used by the daemonizable service.")]
    private readonly UpdatingPrimaryValue DaemonNamePatternConstant = new();
    private string? CurrentDaemonNamePattern;
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When the control string was formatted badly.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, DaemonNamePatternConstant).IsRereadRequired(out string? patternCandidate))
            CurrentDaemonNamePattern = patternCandidate;
        CurrentDaemonNamePattern ??= "stop ^.*$";

        var splitPattern = CurrentDaemonNamePattern.Split([' '], 2, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).ToArray();
        var operation = splitPattern.ElementAtOrDefault(0) ?? "stop";
        var pattern = splitPattern.ElementAtOrDefault(1) ?? "^.*$";

        switch (operation)
        {
            case "stop":
                foreach (var x in FixedDaemon.Daemons.Where(x => Regex.IsMatch(x.Key, pattern)))
                    x.Value.Stop();
                break;
            case "restart":
                foreach (var x in FixedDaemon.Daemons.Where(x => Regex.IsMatch(x.Key, pattern)))
                    x.Value.Restart();
                break;
            default:
                OnException?.Invoke(this, new CommonInteraction(interaction, "invalid daemon control instruction"));
                break;
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}