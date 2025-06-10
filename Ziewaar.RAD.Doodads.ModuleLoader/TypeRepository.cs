namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class TypeRepository
{
    public static readonly TypeRepository Instance = new();
    private readonly SortedList<string, Type> NamedServiceTypes = new();
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
        var serviceTypes = assembly.
            GetTypes().
            Where(x => typeof(IService).IsAssignableFrom(x) && !x.IsAbstract);
        foreach (var serviceType in serviceTypes)
        {
            NamedServiceTypes.Add(serviceType.Name, serviceType);
        }
        return this;
    }
    public IService CreateInstanceFor(string name, out Type foundType)
    {
        foundType = NamedServiceTypes[name];
        return (IService)Activator.CreateInstance(foundType ??
            throw new MissingServiceTypeException(name));
    }
    public string[] GetAvailableNames() => NamedServiceTypes.Keys.ToArray();
    public bool HasName(string newTypeName) => NamedServiceTypes.ContainsKey(newTypeName);
}
