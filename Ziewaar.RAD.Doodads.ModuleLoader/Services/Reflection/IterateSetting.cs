#nullable enable
using System.Collections;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
#pragma warning disable 67
[Category("Reflection & Documentation")]
[Title("Iterate through subsettings")]
[Description("Iterate through some subsetting in register")]
public class IterateSetting : IteratingService
{
    [EventOccasion("In case there's no suitable settings in the register")]
    public override event CallForInteraction? OnException;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants,
        IInteraction repeater)
    {
        if (repeater.Register is IEnumerable enumerable)
            return enumerable.OfType<object>()
                .Select(repeater.AppendRegister);
        OnException?.Invoke(this,
            repeater.AppendRegister("register must be enumerable for this"));
        return [];
    }
}