#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
#pragma warning disable 67
[Category("Reflection & Documentation")]
[Title("Iterate through a service series")]
[Description("Outputs discriminable services in order of the series")]
public class IterateSeries : IteratingService
{
    [EventOccasion("In case there wasn't a series in scope")]
    public override event CallForInteraction? OnException;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants,
        IInteraction repeater)
    {
        if (repeater.TryFindVariable(ReflectionKeys.Series,
                out SerializableServiceSeries<ServiceBuilder>? series) &&
            series != null)
            return series.Children?.Select(x => repeater.AppendMemory([
                (
                    ReflectionKeys.ServiceExpression, x)
            ])) ?? [];

        OnException?.Invoke(this,
            repeater.AppendRegister("series required for this"));
        return [];
    }
}