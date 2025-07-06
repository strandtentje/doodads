namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public class ValidateForm : IService
{
    private readonly UpdatingPrimaryValue FormNameConst = new();
    private string? CurrentFormName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnValid;
    public event CallForInteraction? OnInvalid;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, FormNameConst).IsRereadRequired(out string? fieldDumpVariable) &&
            !string.IsNullOrWhiteSpace(fieldDumpVariable))
            this.CurrentFormName = fieldDumpVariable;
        if (string.IsNullOrWhiteSpace(this.CurrentFormName))
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "No variable name provided to put composed fields into"));
            return;
        }
        interaction.TryGetClosest(out ISinkingInteraction? originalSinkingInteraction);
        var validationState = new PreValidationStateInteraction(this.CurrentFormName, new TextSinkingInteraction(interaction));
        OnThen?.Invoke(this, validationState);
        OnElse?.Invoke(this, validationState);
        if (validationState.MustValidate && validationState.FieldValidations.All(x => x.Value))
            OnValid?.Invoke(this,
                new PostValidationStateInteraction(
                    CurrentFormName,
                    validationState,
                    originalSinkingInteraction,
                    validationState.FieldValues,
                    validationState.FieldSink));
        else
            OnInvalid?.Invoke(this,
                new PostValidationStateInteraction(
                    CurrentFormName,
                    validationState,
                    originalSinkingInteraction,
                    new SortedList<string, object>(),
                    validationState.FieldSink));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}