#nullable enable
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
              - primary: Name of the primary setting on this service, or empty string if none.
              - shorthand: In case this service has syntactic sugar 
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
            { "shorthand", DocumentationRepository.Instance.GetTypeShorthand(serviceName) ?? "" },
        };
        OnThen?.Invoke(this, new CommonInteraction(interaction, memory: payload));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}