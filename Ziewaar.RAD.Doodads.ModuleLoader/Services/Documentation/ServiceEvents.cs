#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Documentation;
#pragma warning disable 67
[Category("Reflection & Documentation")]
[Title("Enumerate events that service exposes")]
[Description("""
             Iterate event names of the service name currently in register, while
             an appropriate continue is triggered.
             """)]
public class ServiceEvents : IService
{
    [PrimarySetting("Name of Loop")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? RepeatName;
    [EventOccasion("Has an event name in register")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When no continue name was given, or no suitable service name was in the register")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? candidateRepeatName))
            this.RepeatName = candidateRepeatName;
        if (string.IsNullOrWhiteSpace(this.RepeatName) || this.RepeatName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }
        if (interaction.Register is not string serviceName)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Service name required in register"));
            return;
        }
        var typeEventNames = DocumentationRepository.Instance.GetTypeEvents(serviceName);
        var ri = new RepeatInteraction(this.RepeatName, interaction);
        foreach (var eventName in typeEventNames)
        {
            if (!ri.IsRunning) break;
            ri.IsRunning = false;
            OnThen?.Invoke(this, new CommonInteraction(ri, eventName));
        }
        
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}