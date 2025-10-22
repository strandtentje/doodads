#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
#pragma warning disable 67
public class IterateChildren : IteratingService
{
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