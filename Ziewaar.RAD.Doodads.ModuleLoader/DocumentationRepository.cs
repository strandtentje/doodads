#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class DocumentationRepository
{
    // please stop reading while you still can.
    private string[]? categoryStrings;
    private Dictionary<string, string[]> categoryTypes = new(), typeEvents = new(), typeSettings = new();
    private Dictionary<string, string> typeTitles = new(), typeDescriptions = new();
    private Dictionary<(string, string), string> eventDescriptions = new(), settingDescriptions = new();
    private Dictionary<(string, string), string?> namedSettingKeys = new();
    private Dictionary<string, string?> typePrimarySettings = new(),typeShorthands = new();
    public static readonly DocumentationRepository Instance = new();
    private IReadOnlyDictionary<string, Type> NamedServiceTypes => TypeRepository.Instance.NamedServiceTypes;
    public string[] GetCategories() =>
        categoryStrings ??=
            NamedServiceTypes.Values.Select(x => x.GetCustomAttribute<CategoryAttribute>()?.Name).OfType<string>()
                .Distinct().OrderBy(x => x).ToArray();
    public string[] GetCategoryTypes(string category) =>
        categoryTypes.TryGetValue(category, out var typeNames)
            ? typeNames
            : categoryTypes[category] = NamedServiceTypes.Values.Where(x =>
                    x.GetCustomAttributes().OfType<CategoryAttribute>().SingleOrDefault()?.Name == category)
                .Select(x => x.Name).OrderBy(x => x).ToArray();
    public string GetTypeTitle(string typeName) =>
        typeTitles.TryGetValue(typeName, out var title)
            ? title
            : typeTitles[typeName] =
                NamedServiceTypes[typeName].GetCustomAttribute<TitleAttribute>().Title;
    public string GetTypeDescription(string typeName) =>
        typeDescriptions.TryGetValue(typeName, out var description)
            ? description
            : typeDescriptions[typeName] =
                NamedServiceTypes[typeName].GetCustomAttribute<DescriptionAttribute>().Description;
    public string? GetTypeShorthand(string typeName) =>
        typeShorthands.TryGetValue(typeName, out var format)
            ? format
            : typeShorthands[typeName] =
                NamedServiceTypes[typeName].GetCustomAttributes().OfType<ShorthandAttribute>()
                    .FirstOrDefault()?.Format;
    public string[] GetTypeEvents(string typeName) =>
        typeEvents.TryGetValue(typeName, out var events)
            ? events
            : typeEvents[typeName] =
                NamedServiceTypes[typeName].GetEvents()
                    .Where(x => typeof(CallForInteraction).IsAssignableFrom(x.EventHandlerType) &&
                                !x.GetCustomAttributes().OfType<NeverHappensAttribute>().Any()).Select(x => x.Name)
                    .OrderBy(x => x)
                    .ToArray();
    public string GetEventDescription(string typeName, string eventName) =>
        eventDescriptions.TryGetValue((typeName, eventName), out var description)
            ? description
            : eventDescriptions[(typeName, eventName)] =
                NamedServiceTypes[typeName].GetEvents()
                    .Single(x => typeof(CallForInteraction).IsAssignableFrom(x.EventHandlerType) && x.Name == eventName)
                    .GetCustomAttribute<EventOccasionAttribute>().EventOccasion;
    public string? GetTypePrimarySetting(string typeName) =>
        typePrimarySettings.TryGetValue(typeName, out var setting)
            ? setting
            : typePrimarySettings[typeName] =
                NamedServiceTypes[typeName]
                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .SingleOrDefault(x => typeof(UpdatingPrimaryValue).IsAssignableFrom(x.FieldType))?.Name;
    public string[] GetTypeNamedSettings(string typeName) =>
        typeSettings.TryGetValue(typeName, out var settings)
            ? settings
            : typeSettings[typeName] =
                NamedServiceTypes[typeName].GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => typeof(IUpdatingValue).IsAssignableFrom(x.FieldType)).Select(x => x.Name)
                    .OrderBy(x => x).ToArray();
    public string? GetNamedSettingKey(string typeName, string settingName) =>
        namedSettingKeys.TryGetValue((typeName, settingName), out var key)
            ? key
            : namedSettingKeys[(typeName, settingName)] =
                NamedServiceTypes[typeName].GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .SingleOrDefault(x => typeof(IUpdatingValue).IsAssignableFrom(x.FieldType) && x.Name == settingName)
                    ?
                    .GetCustomAttribute<NamedSettingAttribute>()?.Key;
    public string GetSettingDescription(string typeName, string settingName) =>
        settingDescriptions.TryGetValue((typeName, settingName), out var description)
            ? description
            : settingDescriptions[(typeName, settingName)] =
                NamedServiceTypes[typeName].GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Single(x => typeof(IUpdatingValue).IsAssignableFrom(x.FieldType) && x.Name == settingName)
                    .GetCustomAttributes().OfType<IHaveText>().SingleOrDefault()?.Text ?? "Undocumented";
}