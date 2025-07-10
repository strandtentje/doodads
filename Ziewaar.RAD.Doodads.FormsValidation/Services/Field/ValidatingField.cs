using Ziewaar.RAD.Doodads.FormsValidation.Common;
using Ziewaar.RAD.Doodads.FormsValidation.Interactions;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Field;
#pragma warning disable 67
public abstract class ValidatingField<TDefault> : IService
{
    [PrimarySetting("""
                    Human-readable title for this field;
                    Note this will be used for memory names and field names such 
                    that everything that isn't a letter or a digit gets turned into an underscore.
                    """)]
    private readonly UpdatingPrimaryValue FieldTitleConstant = new();
    [NamedSetting("default", "Value to default to if it wasn't provided")]
    private readonly UpdatingKeyValue DefaultValueConstant = new("default");
    [NamedSetting("required", """
                              If the value wasn't provided means invalidation; otherwise default value 
                              will be validated
                              """)]
    private readonly UpdatingKeyValue RequiredConstant = new("required");
    [NamedSetting("range", """
                           Special setting for the particular field type or a dash (-) separated
                           pair for min and max such that `min-max` ie. 0-100.
                           """)]
    private readonly UpdatingKeyValue RangeConstant = new("range");
    [NamedSetting("min", """
                         Minimum value (or length in case of text field)
                         """)]
    private readonly UpdatingKeyValue MinConstant = new("min");
    [NamedSetting("max", """
                         Maximum value (or length in case of text field)
                         """)]
    private readonly UpdatingKeyValue MaxConstant = new("max");
    [NamedSetting("nest", """
                          Set this to true if static validation is not enough;
                          in that case, use in conjunction with AcceptValidation and RejectValidation
                          """)]
    private readonly UpdatingKeyValue IsNestingConstant = new("nest");
    private string? FixedCurrentFieldTitle;
    private string? FixedSanitizedFieldTitle;
    private object? FixedCurrentDefault;
    private bool IsCurrentlyRequired;
    private bool IsCurrentlyNesting;
    [EventOccasion("""
                   When nest is enabled, the validation value ends up in register here. 
                   Use AcceptValidation or RejectValidation to ultimately validate.
                   """)]
    public event CallForInteraction? OnThen;
    [EventOccasion("""
                   Sink the name in case no default was set.
                   """)]
    public event CallForInteraction? GetName;
    [EventOccasion("""When no default was hardcoded, sink it here.""")]
    public event CallForInteraction? OnElse;
    [EventOccasion("""
                   Could happen when
                    - Title was not set
                    - We're not doing this in a ValidateForm block
                    - Nesting validation didn't produce results.
                   """)]
    public event CallForInteraction? OnException;
    [EventOccasion("""
                   When this particular field is definitively valid and will be known to ValidateForm
                   """)]
    public event CallForInteraction? OnValid;
    [EventOccasion("""
                   When this particular field is not valid and further validation will not result in
                   ValidateForm succeeding
                   """)]
    public event CallForInteraction? OnInvalid;
    [EventOccasion("""
                   When we are not in a Validation cycle, but rather in a preparation cycle ie.
                   Displaying the form.
                   """)]
    public event CallForInteraction? OnInitial;
    protected abstract void SetLowerBoundary(object? boundary);
    protected abstract void SetUpperBoundary(object? boundary);
    protected virtual void SetPrimaryConstraint(object? rangeCandidate)
    {
        var items = rangeCandidate?.ToString()?.Split('-', (StringSplitOptions)3) ?? [];
        SetLowerBoundary(items.ElementAtOrDefault(0));
        SetUpperBoundary(items.ElementAtOrDefault(1));
    }
    protected abstract bool TryValidate(StampedMap constants, string valueToValidate,
        [NotNullWhen(true)] out object? validatedValue);
    protected virtual object? GetDefault(StampedMap constants, IInteraction parent)
    {
        if ((constants, DefaultValueConstant).IsRereadRequired(out object? defaultCandidate))
            this.FixedCurrentDefault = defaultCandidate ?? default(TDefault);
        string CurrentDefault;
        if (this.FixedCurrentDefault is not string dflt || string.IsNullOrWhiteSpace(dflt))
        {
            var tsi = new TextSinkingInteraction(parent);
            OnElse?.Invoke(this, tsi);
            CurrentDefault = tsi.ReadAllText();
        } else
        {
            CurrentDefault = dflt;
        }
        if (TryValidate(constants, CurrentDefault.ToString() ?? "", out var validatedDefault))
            return validatedDefault;
        else
            return default(TDefault);
    }
    protected virtual bool IsRequired(StampedMap constants)
    {
        if ((constants, RequiredConstant).IsRereadRequired(() => true, out bool? isRequired))
            this.IsCurrentlyRequired = isRequired == true;
        return this.IsCurrentlyRequired;
    }
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, FieldTitleConstant).IsRereadRequired(out string? newFieldTitle) &&
            newFieldTitle != null)
        {
            newFieldTitle = newFieldTitle.ToLower();
            this.FixedCurrentFieldTitle = newFieldTitle;
            this.FixedSanitizedFieldTitle = newFieldTitle.Alphanumerize();
        }
        string SanitizedFieldTitle, CurrentFieldTitle;
        if (this.FixedSanitizedFieldTitle == null || this.FixedCurrentFieldTitle == null ||
            string.IsNullOrWhiteSpace(FixedSanitizedFieldTitle) || string.IsNullOrWhiteSpace(FixedCurrentFieldTitle))
        {
            var tsi = new TextSinkingInteraction(interaction);
            GetName?.Invoke(this, tsi);
            var sourcedName = tsi.ReadAllText();
            if (string.IsNullOrWhiteSpace(sourcedName))
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "field title is required for this"));
                return;
            } else
            {
                (SanitizedFieldTitle, CurrentFieldTitle) = (sourcedName.Trim().ToLower().Alphanumerize(), sourcedName);
            }
        } else
        {
            (SanitizedFieldTitle, CurrentFieldTitle) = (FixedSanitizedFieldTitle, FixedCurrentFieldTitle);
        }
        if (!interaction.TryGetClosest(
                out PreValidationStateInteraction? preValidationState) ||
            preValidationState == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "Use this in conjunction with a validation block like ValidateForm"));
            return;
        }
        if ((constants, MinConstant).IsRereadRequired(out object? minValueCandidate))
            SetLowerBoundary(minValueCandidate);
        if ((constants, MaxConstant).IsRereadRequired(out object? maxValueCandidate))
            SetUpperBoundary(maxValueCandidate);
        if ((constants, RangeConstant).IsRereadRequired(out string? rangeCandidate))
            SetPrimaryConstraint(rangeCandidate);

        var formName = preValidationState.FormName;

        interaction.TryGetClosest(out ICsrfTokenSourceInteraction? csrfTokenSourceInteraction);

        if (preValidationState.ProceedAt != null)
        {
            preValidationState.FieldValidations[SanitizedFieldTitle] = false;
            var workingFieldName = SanitizedFieldTitle;
            if (csrfTokenSourceInteraction != null)
            {
                if (csrfTokenSourceInteraction.Fields.TryObfuscating(formName,
                        workingFieldName, out var obfuscatedFieldName) &&
                    !string.IsNullOrWhiteSpace(obfuscatedFieldName))
                {
                    workingFieldName = obfuscatedFieldName;
                }
                else
                {
                    OnInvalid?.Invoke(this, new FieldPropertiesInteraction(
                        interaction, SanitizedFieldTitle,
                        MaskField(SanitizedFieldTitle, csrfTokenSourceInteraction, formName),
                        CurrentFieldTitle, CreateDefaultValue(constants, interaction)));
                    return;
                }
            }

            if (!preValidationState.ProceedAt.TryFindVariable(workingFieldName, out object? foundFieldValue) ||
                foundFieldValue?.ToString() is not string candidateFieldText ||
                string.IsNullOrWhiteSpace(candidateFieldText))
            {
                if (IsRequired(constants))
                {
                    OnInvalid?.Invoke(this, new FieldPropertiesInteraction(
                        interaction, SanitizedFieldTitle,
                        MaskField(SanitizedFieldTitle, csrfTokenSourceInteraction, formName),
                        CurrentFieldTitle, CreateDefaultValue(constants, interaction)));
                    return;
                }
                else
                {
                    candidateFieldText = GetDefault(constants, interaction)?.ToString() ?? "";
                }
            }

            if (TryValidate(constants, candidateFieldText, out var validatedValue))
            {
                if ((constants, IsNestingConstant).IsRereadRequired(out bool? isNesting))
                    this.IsCurrentlyNesting = isNesting == true;
                bool isValid = true;
                if (IsCurrentlyNesting)
                {
                    var extraValidationInteraction = new NestingValidationInteraction(interaction, validatedValue);
                    OnThen?.Invoke(this, extraValidationInteraction);
                    switch (extraValidationInteraction.Validity)
                    {
                        case Tristate.Unknown:
                            OnException?.Invoke(this,
                                new CommonInteraction(interaction,
                                    """
                                    Nesting validation had no result.
                                     Set nest=false or use RejectValidation/AcceptValidation
                                    """));
                            return;
                        case Tristate.False:
                            isValid = false;
                            break;
                        case Tristate.True:
                            isValid = true;
                            break;
                    }
                }
                preValidationState.FieldValidations[SanitizedFieldTitle] = isValid;
                if (isValid)
                {
                    preValidationState.FieldValues[SanitizedFieldTitle] = validatedValue;
                    OnValid?.Invoke(this, new FieldPropertiesInteraction(
                        interaction,
                        SanitizedFieldTitle,
                        MaskField(SanitizedFieldTitle, csrfTokenSourceInteraction, formName),
                        CurrentFieldTitle, validatedValue.ToString() ?? ""));
                    return;
                }
            }

            OnInvalid?.Invoke(this, new FieldPropertiesInteraction(
                interaction, SanitizedFieldTitle,
                MaskField(SanitizedFieldTitle, csrfTokenSourceInteraction, formName),
                CurrentFieldTitle, CreateDefaultValue(constants, interaction)));
        }
        else
        {
            OnInitial?.Invoke(this, new FieldPropertiesInteraction(
                interaction, SanitizedFieldTitle,
                csrfTokenSourceInteraction?.Fields.NewObfuscation(formName, SanitizedFieldTitle) ??
                SanitizedFieldTitle,
                CurrentFieldTitle, ""));
        }
    }
    private string
        MaskField(string unmasked, ICsrfTokenSourceInteraction? csrfTokenSourceInteraction, string formName) =>
        csrfTokenSourceInteraction?.Fields.NewObfuscation(formName, unmasked) ?? unmasked;
    private string CreateDefaultValue(StampedMap constants, IInteraction interaction) => GetDefault(constants, interaction)?.ToString() ?? "";
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}