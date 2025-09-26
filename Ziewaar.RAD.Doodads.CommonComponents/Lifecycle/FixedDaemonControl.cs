#nullable enable
using System.Text.RegularExpressions;

namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;
public class FixedDaemonControl : IService
{
    private readonly UpdatingPrimaryValue DaemonNamePatternConstant = new();
    private string? CurrentDaemonNamePattern;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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