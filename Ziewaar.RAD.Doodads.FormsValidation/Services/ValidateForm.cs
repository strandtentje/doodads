namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
[Category("HTTP Forms")]
[Title("Form and Validation Scope")]
[Description("""
             Main Service for coordinating validation and forms.
             First triggers data parsing, Then triggers validation.
             Will Validate or not depending on the Fields connected.
             """)]
public class ValidateForm : IService
{
    [PrimarySetting("Form name to use for the CSRF token registry, and for putting the form body text into")]
    private readonly UpdatingPrimaryValue FormNameConst = new();
    private string? CurrentFormName;
    [EventOccasion("""
                   Connect services here to figure out if validation needs to happen,
                   and what data needs to be validated. ie.
                   HttpMethod("POST"):Route("/some-form"):ReadUrlEncodedBody():RequireValidation()
                   """)]
    public event CallForInteraction? OnThen;
    [EventOccasion("""
                   Connect services here to conduct validation. ie.
                   TextField("Name", range = "3-100", required = true) {
                        OnInitial->PrintPlaceholderInput(f"");
                        OnValid->PrintFilledInput(f"");
                        OnInvalid->PrintErrorInput(f"");
                   } &
                   DateField("Birthday", range = "0-100", required = true) {
                       OnInitial->PrintPlaceholderDateField(f"");
                       OnValid->PrintFilledDateField(f"");
                       OnInvalid->PrintErrorDateField(f"");
                   }
                   """)]
    public event CallForInteraction? OnElse;
    [EventOccasion("""
                   Continues here if all the field were validated to be correct.
                   Will have the values under the appropriate names in memory.
                   """)]
    public event CallForInteraction? OnValid;
    [EventOccasion("""
                   Continues here if validation was attempted but it was all wrong.
                   It is typical to put a Format("{% name_of_the_form %}") here, to 
                   output the form and its errors. Or an HttpStatus(400) in case 
                   it's not a UX.
                   """)]
    public event CallForInteraction? OnInvalid;
    [EventOccasion("Likely happens when no form name was provided.")]
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