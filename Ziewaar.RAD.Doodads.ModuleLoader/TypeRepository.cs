#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class TypeRepository
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
            serviceTypes = typeof(IService).Assembly.GetTypes().Where(x => typeof(IService).IsAssignableFrom(x) && !x.IsAbstract);
        else if (typeof(TypeRepository).Assembly.FullName == assembly.FullName)
            serviceTypes = typeof(TypeRepository).Assembly.GetTypes().Where(x => typeof(IService).IsAssignableFrom(x) && !x.IsAbstract);
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
            }
        }
        return this;
    }
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
            return (IService)Activator.CreateInstance(foundType);
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

}