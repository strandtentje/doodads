using Ziewaar.RAD.Doodads.FormsValidation.Interactions;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services;
#pragma warning disable 67
[Category("HTTP Forms")]
[Title("Ready to Validate")]
[Description("""
             Generally used after ValidateForm, Route and HttpMethod to decide it's 
             time to validate a form.
             """)]
public class RequireValidation : IService
{
    [EventOccasion("Simply continues")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when no form was in scope for validation")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<PreValidationStateInteraction>(out var preValidationState) || 
            preValidationState == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, $"May only be called via {nameof(ValidateForm)}"));
            return;
        }
        preValidationState.ProceedAt = interaction;
        OnThen?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}