using Ziewaar.RAD.Doodads.FormsValidation.Common;
using Ziewaar.RAD.Doodads.FormsValidation.Interactions;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Dynamic;
#pragma warning disable 67
public abstract class ModifyValidation : IService
{
    [EventOccasion("Always continues")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When used outside of scope")]
    public event CallForInteraction? OnException;
    protected abstract bool GetValidity(StampedMap constants, IInteraction interaction);
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<NestingValidationInteraction>(out var nesting) || nesting == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "This may only be used with a Field that has nest set to true"));
            return;
        }
        Tristate tristate = GetValidity(constants, interaction) ? Tristate.True : Tristate.False;
        nesting.AndValidity &= tristate;
        nesting.OrValidity |= tristate;
        OnThen?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
