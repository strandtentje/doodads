#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Documentation;
[Category("Reflection & Documentation")]
[Title("Get detailed event description by service name and event name")]
[Description("""
             Provided an event name in the register, and service name in memory at 'service',
             or with the service name as the primary setting, and the event name as the 'event' settings,
             puts the description of the event in the register.
             """)]
public class EventDescription : IService
{
    [PrimarySetting("Optionally hardcoded service name")]
    private readonly UpdatingPrimaryValue ServiceName = new();
    [NamedSetting("event", "Optionally hardcoded event name")]
    private readonly UpdatingKeyValue EventName = new("event");
    [EventOccasion("When the event was found, has description in register")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [EventOccasion("When the service name or event name were not available via settings or interaction")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, ServiceName).IsRereadRequired<string>(out var hardcodedServiceName);
        (constants, EventName).IsRereadRequired<string>(out var hardcodedEventName);
        var serviceName = hardcodedServiceName;
        var eventName = hardcodedEventName;
        if (interaction.TryFindVariable<string>("service", out var interactionServiceName))
            serviceName = interactionServiceName;
        if (interaction.Register is string interactionEventName)
            eventName = interactionEventName;
        if (serviceName == null || eventName == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "service and event name required via register/memory or primary setting"));
            return;
        }
        OnThen?.Invoke(this,
            new CommonInteraction(interaction,
                DocumentationRepository.Instance.GetEventDescription(serviceName, eventName)));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}