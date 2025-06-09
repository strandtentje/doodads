namespace Ziewaar.RAD.Doodads.ModuleLoader;

[Serializable]
public class MissingServiceTypeException : Exception
{
    public MissingServiceTypeException(string name) : base($"No service type `{name}` was found. Are you sure all assemblies have been loaded")
    {
    }
    protected MissingServiceTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
