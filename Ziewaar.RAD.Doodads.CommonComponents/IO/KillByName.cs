#nullable enable
#pragma warning disable 67
using System.Diagnostics;

namespace Ziewaar.RAD.Doodads.CommonComponents.IO;

[Category("System & IO")]
[Title("Kill programme by name")]
[Description("""
    Find a program by executable name and kill it.
    """)]
public class KillByName : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

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
                    GlobalLog.Instance?.Warning(ex, "Couldn't kill process {pid} - {exe}", item.Id, item.ProcessName);
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
