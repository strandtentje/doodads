using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

namespace Ziewaar.RAD.Doodads.RuntimeForDotnetCore;

internal class DateOnlyAdaptor : ITypeAdaptor
{
    private DateOnlyAdaptor()
    {
    }
    public static readonly DateOnlyAdaptor Instance = new();
    public Type TypeToAdapt { get; } = typeof(DateOnly);
    public object DownConvert(object source) => ((DateOnly)source).ToDateTime(new TimeOnly());
}