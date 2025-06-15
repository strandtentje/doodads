#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Reflection;
[Category("Reflection")]
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
[Category("Reflection")]
[Title("Get all the definitions that exist in the file.")]
[Description("""
             Provided with a full file path in Register, will enumerate the definitions that exist in it.
             """)]
public class DefinitionsInFile : IService
{
    [PrimarySetting("Optionally hardcoded rkop path")]
    private readonly UpdatingPrimaryValue ServicePath = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, ServicePath).IsRereadRequired<string>(out var hardcodedServicePath);
        var requestedPath = hardcodedServicePath ?? interaction.Register as string;
        if (interaction.TryFindVariable("path", out string? memoryPath))
            requestedPath = memoryPath;
        if (requestedPath == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "rkop path must be provided via memory, register or configuration."));
            return;
        }
        var definitionNames = ProgramRepository.Instance.GetForFile(requestedPath).Definitions?.Select(x => x.Name)
            ?.ToArray() ?? [];
        OnThen?.Invoke(this, new CommonInteraction(interaction, definitionNames, new SortedList<string, object>()
        {
            { "path", requestedPath }
        }));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
public class ServiceInDefinition : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryFindVariable("path", out string? requestedPath) ||
            requestedPath == null ||
            interaction.Register is not string definitionName)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "path in memory and definition name in register required"));
            return;
        }
        var definition = ProgramRepository.Instance.GetForFile(requestedPath).Definitions
            ?.SingleOrDefault(x => x.Name == definitionName);
        if (definition == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "definition not found"));
            return;
        }
        OnThen?.Invoke(this, new CommonInteraction(interaction, definition.CurrentSeries));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
public class ServiceDiscriminator : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public event CallForInteraction? OnSeries;
    public event CallForInteraction? OnCoalesce;
    public event CallForInteraction? OnRedirect;
    public event CallForInteraction? OnDefinition;
    
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is not ServiceExpression<ServiceBuilder> bld)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "service expression expected in register"));
            return;
        }
        bld.
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}