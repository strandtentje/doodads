#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Documentation;
[Category("Reflection & Documentation")]
[Title("Get information of a service by its name")]
[Description("""
             Provided a service name through the primary setting or the Register, it will put information 
             about this service in memory;
              - service: The full name of the service as it was used to query this service.
              - title : Documentation title of service 
              - description : Documentation description of service in markdown
              - events: List of event names that may occur on this service
              - primary: Name of the primary setting on this service, or empty string if none.
              - named: List of names of the named settings on this service 
             Also puts the service name in register.
             """)]
public class ServiceHeader : IService
{
    [PrimarySetting("Optionally hardcoded category name")]
    private readonly UpdatingPrimaryValue ServiceName = new();
    [EventOccasion("Contains the payload as described for the requested service")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no service name was provided via register or primary setting")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, ServiceName).IsRereadRequired<string>(out var hardcodedServiceName);
        var serviceName = hardcodedServiceName ?? interaction.Register as string;
        if (serviceName == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "service name required via register or primary setting"));
            return;
        }
        var payload = new SortedList<string, object>()
        {
            { "service", serviceName },
            { "title", DocumentationRepository.Instance.GetTypeTitle(serviceName) },
            { "description", DocumentationRepository.Instance.GetTypeDescription(serviceName) },
            { "primary", DocumentationRepository.Instance.GetTypePrimarySetting(serviceName) ?? "" },
        };
        OnThen?.Invoke(this, new CommonInteraction(interaction, memory: payload));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class ServiceEvents : IService
{
    [PrimarySetting("Name of Loop")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? RepeatName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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

public class ServiceSettings : IService
{
    [PrimarySetting("Name of Loop")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? RepeatName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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

