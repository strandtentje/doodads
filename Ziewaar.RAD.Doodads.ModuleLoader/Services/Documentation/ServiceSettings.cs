#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Documentation;
#pragma warning disable 67
[Category("Reflection & Documentation")]
[Title("Enumerate settings that the service exposes.")]
[Description("""
             Provided a service name in register, loop through
             the settings it exposes.
             """)]
public class ServiceSettings : IService
{
    [PrimarySetting("Name of Loop")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? RepeatName;
    [EventOccasion("Loops through setting names in register, while an appropriate Continue happens")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When no continue name was given, or no service name was in register")]
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
        var typeSettingNames = DocumentationRepository.Instance.GetTypeNamedSettings(serviceName);
        var ri = new RepeatInteraction(this.RepeatName, interaction);
        foreach (var settingName in typeSettingNames)
        {
            if (!ri.IsRunning) break;
            ri.IsRunning = false;
            OnThen?.Invoke(this, new CommonInteraction(ri, settingName));
        }
        
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}