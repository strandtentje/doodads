using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

namespace Ziewaar.RAD.Doodads.RuntimeForDotnetCore;

internal class TimeOnlyAdaptor : ITypeAdaptor
{
    private TimeOnlyAdaptor()
    {
    }
    public static readonly TimeOnlyAdaptor Instance = new();
    public Type TypeToAdapt { get; } = typeof(TimeOnly);
    public object DownConvert(object source) => ((TimeOnly)source).ToTimeSpan();
}