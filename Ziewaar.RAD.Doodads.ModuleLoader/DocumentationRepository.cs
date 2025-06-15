#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class DocumentationRepository
{
    public static readonly DocumentationRepository Instance = new();
    private IReadOnlyDictionary<string, Type> NamedServiceTypes => TypeRepository.Instance.NamedServiceTypes;
    public string[] GetCategories() =>
        NamedServiceTypes.Values.Select(x => x.GetCustomAttribute<CategoryAttribute>()?.Name).OfType<string>()
            .Distinct().ToArray();
    public string[] GetCategoryTypes(string category) =>
        NamedServiceTypes.Values.Where(x => x.GetCustomAttribute<CategoryAttribute>().Name == category)
            .Select(x => x.Name).ToArray();
    public string GetTypeTitle(string typeName) =>
        NamedServiceTypes[typeName].GetCustomAttribute<TitleAttribute>().Title;
    public string GetTypeDescription(string typeName) =>
        NamedServiceTypes[typeName].GetCustomAttribute<DescriptionAttribute>().Description;
    public string[] GetTypeEvents(string typeName) =>
        NamedServiceTypes[typeName].GetEvents()
            .Where(x => typeof(CallForInteraction).IsAssignableFrom(x.EventHandlerType) &&
                        !x.GetCustomAttributes().OfType<NeverHappensAttribute>().Any()).Select(x => x.Name).ToArray();
    public string GetEventDescription(string typeName, string eventName) =>
        NamedServiceTypes[typeName].GetEvents()
            .Single(x => typeof(CallForInteraction).IsAssignableFrom(x.EventHandlerType) && x.Name == eventName)
            .GetCustomAttribute<EventOccasionAttribute>().EventOccasion;
    public string? GetTypePrimarySetting(string typeName) =>
        NamedServiceTypes[typeName]
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .SingleOrDefault(x => typeof(UpdatingPrimaryValue).IsAssignableFrom(x.FieldType))?.Name;
    public string[] GetTypeNamedSettings(string typeName) =>
        NamedServiceTypes[typeName].GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => typeof(UpdatingKeyValue).IsAssignableFrom(x.FieldType)).Select(x => x.Name).ToArray();
    public string? GetNamedSettingKey(string typeName, string settingName) =>
        NamedServiceTypes[typeName].GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .SingleOrDefault(x => typeof(UpdatingKeyValue).IsAssignableFrom(x.FieldType) && x.Name == settingName)?
            .GetCustomAttribute<NamedSettingAttribute>()?.Key;
    public string GetSettingDescription(string typeName, string settingName) =>
        NamedServiceTypes[typeName].GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(x => typeof(IUpdatingValue).IsAssignableFrom(x.FieldType) && x.Name == settingName)
            .GetCustomAttributes().OfType<IHaveText>().Single().Text;
}