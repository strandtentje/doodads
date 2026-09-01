#pragma warning disable 67
using Ejije.Logging;
using System.Diagnostics;

namespace Ziewaar.RAD.Doodads.CommonComponents.Processes;

[Category("System & IO")]
[Title("Kill programme by name")]
[Description("""
    Find a program by executable name and kill it.
    Use the register for the search word, which may be contained in the 
    process name and is not case sensitive.
    """)]
public class KillByName : IService
{
    [EventOccasion("For each process that was killed")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When no processes were killed")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Process name (or part thereof) wasn't present in register")]
    public event CallForInteraction? OnException;
    private static readonly Log
        FailedToKill = Log.Warn("Failed to kill {pid} of {exe} due to {exception}");
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var exeName = interaction.Register?.ToString();
        if (exeName == null || string.IsNullOrWhiteSpace(exeName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "name required in register"));
            return;
        }
        var processesToKill = Process.GetProcesses().Where(x => x.ProcessName.ToLower().Contains(exeName.ToLower())).ToArray();
        if (processesToKill.Length > 0)
        {
            foreach (var item in processesToKill)
            {
                try
                {
                    item.Kill();
                } catch(Exception ex)
                {
                    Log.Post(FailedToKill, item.Id, item.ProcessName, ex);
                    OnException?.Invoke(this, new CommonInteraction(interaction, ex.Message));
                }
                OnThen?.Invoke(this, interaction);
            }
        } else
        {
            OnElse?.Invoke(this, interaction);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
