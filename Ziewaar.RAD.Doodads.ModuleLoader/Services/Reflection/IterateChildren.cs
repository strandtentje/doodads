#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
#pragma warning disable 67
[Category("Reflection & Documentation")]
[Title("Iterate through named child branches of a service")]
[Description("Services between there accolades may have 0..n other series with sequences, assigned to a name like->so. This will iterate those.")]
public class IterateChildren : IteratingService
{
    [EventOccasion("Likely happens when there wasn't a service in scope")]
    public override event CallForInteraction? OnException;

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants,
        IInteraction repeater)
    {
        if (repeater.TryFindVariable(ReflectionKeys.Service,
                out ServiceDescription<ServiceBuilder>? description) &&
            description != null)
            return description.Children.Branches?.Select(x =>
                repeater.AppendMemory([
                    (ReflectionKeys.ServiceBranchName, x.key),
                    (ReflectionKeys.ServiceExpression, x.value)
                ])) ?? [];
        OnException?.Invoke(this,
            repeater.AppendRegister("service required for this"));
        return [];
    }
}