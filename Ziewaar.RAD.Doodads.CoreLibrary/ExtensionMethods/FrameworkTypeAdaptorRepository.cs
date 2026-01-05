namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

public class FrameworkTypeAdaptorRepository
{
    private FrameworkTypeAdaptorRepository()
    {
        
    }
    public static readonly FrameworkTypeAdaptorRepository Instance = new();
    private readonly List<ITypeAdaptor> TypeAdaptors = new();
    public bool TryConvert(object inObject, out object outObject)
    {
        if (TypeAdaptors.FirstOrDefault(x => x.TypeToAdapt.IsInstanceOfType(inObject)) is { } fittingAdaptor)
        {
            outObject = fittingAdaptor.DownConvert(inObject);
            return true;
        }
        else
        {
            outObject = inObject;
            return false;
        }
    }

    public FrameworkTypeAdaptorRepository Register(ITypeAdaptor adaptor)
    {
        if (TypeAdaptors.Any(x => x.TypeToAdapt == adaptor.TypeToAdapt))
            throw new ArgumentException("Another adaptor was already defined for this type.");
        TypeAdaptors.Add(adaptor);
        return this;
    }
}