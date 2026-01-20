#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class DocumentationRepository
{
    // please stop reading while you still can.
    private string[]? CategoryStrings;
    private readonly Dictionary<string, string[]> CategoryTypes = new();
    private readonly Dictionary<string, string[]> TypeEvents = new();
    private readonly Dictionary<string, string[]> TypeSettings = new();
    private readonly Dictionary<string, string[]> TypeShortNames = new();
    private readonly Dictionary<string, string> TypeTitles = new();
    private readonly Dictionary<string, string> TypeDescriptions = new();
    private readonly Dictionary<(string, string), string> EventDescriptions = new();
    private readonly Dictionary<(string, string), string> SettingDescriptions = new();
    private readonly Dictionary<(string, string), string?> NamedSettingKeys = new();
    private readonly Dictionary<string, string?> TypePrimarySettings = new();
    private readonly Dictionary<string, string?> TypeShorthands = new();
    public static readonly DocumentationRepository Instance = new();
    private IReadOnlyDictionary<string, Type> NamedServiceTypes => TypeRepository.Instance.NamedServiceTypes;

    public string[] GetCategories() =>
        CategoryStrings ??=
            NamedServiceTypes.Values.Select(x => x.GetCustomAttribute<CategoryAttribute>()?.Name).OfType<string>()
                .Distinct().OrderBy(x => x).ToArray();

    public string[] GetCategoryTypes(string category) =>
        CategoryTypes.TryGetValue(category, out var typeNames)
            ? typeNames
            : CategoryTypes[category] = NamedServiceTypes.Values.Where(x =>
                    x.GetCustomAttributes().OfType<CategoryAttribute>().SingleOrDefault()?.Name == category)
                .Select(x => x.Name).OrderBy(x => x).ToArray();

    public string GetTypeTitle(string typeName) =>
        TypeTitles.TryGetValue(typeName, out var title)
            ? title
            : TypeTitles[typeName] =
                NamedServiceTypes[typeName].GetCustomAttribute<TitleAttribute>().Title;

    public string GetTypeDescription(string typeName) =>
        TypeDescriptions.TryGetValue(typeName, out var description)
            ? description
            : TypeDescriptions[typeName] =
                NamedServiceTypes[typeName].GetCustomAttribute<DescriptionAttribute>().Description;

    public string? GetTypeShorthand(string typeName) =>
        TypeShorthands.TryGetValue(typeName, out var format)
            ? format
            : TypeShorthands[typeName] =
                NamedServiceTypes[typeName].GetCustomAttributes().OfType<ShorthandAttribute>()
                    .FirstOrDefault()?.Format;

    public string[] GetTypeShortnames(string typeName) =>
        TypeShortNames.TryGetValue(typeName, out var names)
            ? names
            : TypeShortNames[typeName] =
                NamedServiceTypes[typeName].GetCustomAttributes().OfType<ShortNamesAttribute>().FirstOrDefault()
                    ?.Names ?? [];

    public string[] GetTypeEvents(string typeName) =>
        TypeEvents.TryGetValue(typeName, out var events)
            ? events
            : TypeEvents[typeName] =
                NamedServiceTypes[typeName].GetEvents()
                    .Where(x => typeof(CallForInteraction).IsAssignableFrom(x.EventHandlerType) &&
                                !x.GetCustomAttributes().OfType<NeverHappensAttribute>().Any()).Select(x => x.Name)
                    .OrderBy(x => x)
                    .ToArray();

    public string GetEventDescription(string typeName, string eventName) =>
        EventDescriptions.TryGetValue((typeName, eventName), out var description)
            ? description
            : EventDescriptions[(typeName, eventName)] =
                NamedServiceTypes[typeName].GetEvents()
                    .Single(x => typeof(CallForInteraction).IsAssignableFrom(x.EventHandlerType) && x.Name == eventName)
                    .GetCustomAttribute<EventOccasionAttribute>().EventOccasion;

    public string? GetTypePrimarySetting(string typeName) =>
        TypePrimarySettings.TryGetValue(typeName, out var setting)
            ? setting
            : TypePrimarySettings[typeName] =
                NamedServiceTypes[typeName]
                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .SingleOrDefault(x => typeof(UpdatingPrimaryValue).IsAssignableFrom(x.FieldType))?.Name;

    public string[] GetTypeNamedSettings(string typeName) =>
        TypeSettings.TryGetValue(typeName, out var settings)
            ? settings
            : TypeSettings[typeName] =
                NamedServiceTypes[typeName].GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => typeof(IUpdatingValue).IsAssignableFrom(x.FieldType)).Select(x => x.Name)
                    .OrderBy(x => x).ToArray();

    public string? GetNamedSettingKey(string typeName, string settingName) =>
        NamedSettingKeys.TryGetValue((typeName, settingName), out var key)
            ? key
            : NamedSettingKeys[(typeName, settingName)] =
                NamedServiceTypes[typeName].GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .SingleOrDefault(x => typeof(IUpdatingValue).IsAssignableFrom(x.FieldType) && x.Name == settingName)
                    ?
                    .GetCustomAttribute<NamedSettingAttribute>()?.Key;

    public string GetSettingDescription(string typeName, string settingName) =>
        SettingDescriptions.TryGetValue((typeName, settingName), out var description)
            ? description
            : SettingDescriptions[(typeName, settingName)] =
                NamedServiceTypes[typeName].GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Single(x => typeof(IUpdatingValue).IsAssignableFrom(x.FieldType) && x.Name == settingName)
                    .GetCustomAttributes().OfType<IHaveText>().SingleOrDefault()?.Text ?? "Undocumented";
}