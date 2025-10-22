#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
#pragma warning disable 67
public class IterateSettings : IteratingService
{
    public override event CallForInteraction? OnException;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants,
        IInteraction repeater)
    {
        if (repeater.TryFindVariable(ReflectionKeys.Service,
                out ServiceDescription<ServiceBuilder>? description) &&
            description != null)
            return description.CurrentConstructor?.ConstantsList.Select(x =>
                repeater.AppendRegister(x.Value).AppendMemory([
                    (ReflectionKeys.ServiceSettingName, x.Key)
                ])) ?? [];
        OnException?.Invoke(this,
            repeater.AppendRegister("service required for this"));
        return [];
    }
}