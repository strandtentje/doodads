namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public class RequireValidation : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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
        preValidationState.MustValidate = true;
        OnThen?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}