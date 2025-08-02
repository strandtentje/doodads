#nullable enable
using System.Threading.Tasks;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.ModuleLoader.Exceptions;

namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class TypeRepository : IDisposable
{
    public static readonly TypeRepository Instance = new();
    public readonly SortedList<string, Type> NamedServiceTypes = new();
    private NameSuggestions? Names;
    public TypeRepository PopulateWith(params string[] assemblyFiles)
    {
        foreach (var assemblyPath in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            PopulateWith(assembly);
        }
        return this;
    }
    public TypeRepository PopulateWith(Assembly assembly)
    {
        IEnumerable<Type> serviceTypes;

        if (typeof(IService).Assembly.FullName == assembly.FullName)
            serviceTypes = typeof(IService).Assembly.GetTypes()
                .Where(x => typeof(IService).IsAssignableFrom(x) && !x.IsAbstract);
        else if (typeof(TypeRepository).Assembly.FullName == assembly.FullName)
            serviceTypes = typeof(TypeRepository).Assembly.GetTypes()
                .Where(x => typeof(IService).IsAssignableFrom(x) && !x.IsAbstract);
        else
            serviceTypes = assembly.GetTypes().Where(x => typeof(IService).IsAssignableFrom(x) && !x.IsAbstract);

        foreach (var serviceType in serviceTypes)
        {
            if (NamedServiceTypes.TryGetValue(serviceType.Name, out var alreadyPresent))
            {
                if (alreadyPresent.FullName != serviceType.FullName)
                {
                    throw new Exception("Trying to add a different type under the same name.");
                }
            }
            else
            {
                NamedServiceTypes.Add(serviceType.Name, serviceType);
                var attributes = serviceType.GetCustomAttributes().ToArray();

                Task.Run(() =>
                {
                    if (!attributes.Any(x => x is TitleAttribute) || !attributes.Any(x => x is CategoryAttribute) ||
                        !attributes.Any(x => x is DescriptionAttribute))
                        GlobalLog.Instance?.Warning(
                            "Missing Category, Title or Description attributes on {typeName}",
                            serviceType.Name);

                    foreach (var item in serviceType.GetEvents())
                    {
                        var ca = item.GetCustomAttributes();
                        if (!ca.Any(x => x is EventOccasionAttribute || x is NeverHappensAttribute))
                        {
                            GlobalLog.Instance?.Warning(
                                "Missing EventOccasion or NeverHappens Attribute on {typeName}:{eventName}",
                                serviceType.Name, item.Name);
                        }
                    }

                    var allFields = serviceType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                    foreach (var setting in allFields)
                    {
                        if (typeof(UpdatingPrimaryValue).IsAssignableFrom(setting.FieldType) &&
                            !setting.GetCustomAttributes().Any(x => x is PrimarySettingAttribute))
                            GlobalLog.Instance?.Warning(
                                "Missing PrimarySettingAttribute on {typeName}:{settingName}",
                                serviceType.Name, setting.Name);
                        else if (typeof(UpdatingKeyValue).IsAssignableFrom(setting.FieldType) &&
                                 !setting.GetCustomAttributes().Any(x => x is NamedSettingAttribute))
                            GlobalLog.Instance?.Warning(
                                "Missing NamedSettingAttribute on {typeName}:{settingName}",
                                serviceType.Name, setting.Name);
                    }
                });
            }
        }
        return this;
    }
    List<IDisposable> disposables = new();
    public IService CreateInstanceFor(string name, out Type foundType)
    {
        if (NamedServiceTypes.TryGetValue(name, out var type))
        {
            foundType = NamedServiceTypes[name];
            if (foundType == null)
            {
                if (Names == null)
                    Names = new NameSuggestions(NamedServiceTypes.Keys);
                throw new MissingServiceTypeException(name, Names.GetMostSimilar(name));
            }
            var service = (IService)Activator.CreateInstance(foundType);
            if (service is IDisposable disposableService)
            {
                disposables.Add(disposableService);
            }
            return service;
        }
        else
        {
            if (Names == null)
                Names = new NameSuggestions(NamedServiceTypes.Keys);
            throw new MissingServiceTypeException(name, Names.GetMostSimilar(name));
        }
    }
    public string[] GetAvailableNames() => NamedServiceTypes.Keys.ToArray();
    public bool HasName(string newTypeName) => NamedServiceTypes.ContainsKey(newTypeName);
    public bool TryGetByName(string name, out Type type) => NamedServiceTypes.TryGetValue(name, out type);
    public void Dispose()
    {
        foreach (var item in disposables)
        {
            try
            {
                item.Dispose();
            }
            catch (Exception)
            {
                // ok.
            }
        }
    }
}