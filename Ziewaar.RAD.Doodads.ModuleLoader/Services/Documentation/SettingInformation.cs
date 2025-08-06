#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Documentation;
[Category("Reflection & Documentation")]
[Title("Get detailed settings information by service name and setting name")]
[Description("""
             Provided with a setting name in the register, and a service name in memory at 'service',
             or with those values programmed using the settings, 
             Will output a payload containing
              - setting: the full name of the setting as it was used to query this service 
              - key: the actual key of the setting as its used in the rkop file, unless its a primary setting, then empty.
              - description: the documentative description of the setting
             """)]
public class SettingInformation : IService
{
    [PrimarySetting("Optionally hardcoded service name")]
    private readonly UpdatingPrimaryValue ServiceName = new();
    [NamedSetting("event", "Optionally hardcoded setting name")]
    private readonly UpdatingKeyValue SettingName = new("setting");
    [EventOccasion("When the setting was found, has description in memory")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when the service name or setting name weren't found or provided.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, ServiceName).IsRereadRequired<string>(out var hardcodedServiceName);
        (constants, SettingName).IsRereadRequired<string>(out var hardcodedSettingName);
        var serviceName = hardcodedServiceName;
        var settingName = hardcodedSettingName;
        if (interaction.TryFindVariable<string>("service", out var interactionServiceName))
            serviceName = interactionServiceName;
        if (interaction.Register is string interactionSettingName)
            settingName = interactionSettingName;
        if (serviceName == null || settingName == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "service and event name required via register/memory or primary setting"));
            return;
        }
        OnThen?.Invoke(this,
            new CommonInteraction(interaction, new SortedList<string, object>()
            {
                { "service", serviceName },
                { "setting", settingName },
                { "key", DocumentationRepository.Instance.GetNamedSettingKey(serviceName, settingName) ?? "" },
                { "description", DocumentationRepository.Instance.GetSettingDescription(serviceName, settingName) },
            }));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}