using System.Runtime.InteropServices.ObjectiveC;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public abstract class ValidatingField : IService
{
    private readonly UpdatingPrimaryValue FieldTitleConstant = new();
    private string? CurrentFieldTitle;
    private string? SanitizedFieldTitle;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public event CallForInteraction? OnValid;
    public event CallForInteraction? OnInvalid;
    public event CallForInteraction? OnInitial;
    protected abstract bool TryValidate(StampedMap constants, string valueToValidate, out object validatedValue);
    protected abstract object GetDefault(StampedMap constants);
    protected abstract bool IsRequired(StampedMap constants);
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, FieldTitleConstant).IsRereadRequired(out string? newFieldTitle) &&
            newFieldTitle != null)
        {
            this.CurrentFieldTitle = newFieldTitle;
            this.SanitizedFieldTitle = new string(
                newFieldTitle.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_').ToArray());
        }
        if (this.SanitizedFieldTitle == null || this.CurrentFieldTitle == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "field title is required for this"));
            return;
        }
        if (!interaction.TryGetClosest<PreValidationStateInteraction>(
                out PreValidationStateInteraction? preValidationState) ||
            preValidationState == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "Use this in conjunction with a valiadtion block like ValidateForm"));
            return;
        }

        string formName = preValidationState.FormName;

        interaction.TryGetClosest<CsrfTokenSourceInteraction>(
            out CsrfTokenSourceInteraction? csrfTokenSourceInteraction);

        if (!interaction.TryGetClosest<ISinkingInteraction>(out ISinkingInteraction? sinkingInteraction) ||
            sinkingInteraction == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "Can only use this in conjunction with a sink that takes fields"));
            return;
        }

        string CreateDefaultValue() => GetDefault(constants).ToString() ?? "";
        string MaskField(string unmasked) =>
            csrfTokenSourceInteraction?.Fields.PackField(formName, unmasked) ?? unmasked;

        if (preValidationState.MustValidate)
        {
            preValidationState.FieldValidations[this.SanitizedFieldTitle] = false;
            string workingFieldName = this.SanitizedFieldTitle;
            if (csrfTokenSourceInteraction != null)
            {
                if (!csrfTokenSourceInteraction.Fields.TryRecoverByTrueName(formName,
                        workingFieldName, out workingFieldName))
                {
                    OnInvalid?.Invoke(this, new FieldPropertiesInteraction(
                        sinkingInteraction, this.SanitizedFieldTitle,
                        MaskField(this.SanitizedFieldTitle),
                        this.CurrentFieldTitle, CreateDefaultValue()));
                    return;
                }
            }

            if (!preValidationState.Memory.TryGetValue(workingFieldName, out var foundFieldValue) ||
                foundFieldValue.ToString() is not string candidateFieldText ||
                string.IsNullOrWhiteSpace(candidateFieldText))
            {
                if (IsRequired(constants))
                {
                    OnInvalid?.Invoke(this, new FieldPropertiesInteraction(
                        sinkingInteraction, this.SanitizedFieldTitle,
                        MaskField(this.SanitizedFieldTitle),
                        this.CurrentFieldTitle, CreateDefaultValue()));
                    return;
                }
                else
                {
                    candidateFieldText = GetDefault(constants).ToString() ?? "";
                }
            }

            if (TryValidate(constants, candidateFieldText, out var validatedValue))
            {
                preValidationState.FieldValidations[this.SanitizedFieldTitle] = true;
                preValidationState.FieldValues[this.SanitizedFieldTitle] = validatedValue;
                OnValid?.Invoke(this, new FieldPropertiesInteraction(
                    interaction,
                    this.SanitizedFieldTitle,
                    MaskField(this.SanitizedFieldTitle),
                    this.CurrentFieldTitle, validatedValue.ToString() ?? ""));
            }
            else
            {
                OnInvalid?.Invoke(this, new FieldPropertiesInteraction(
                    sinkingInteraction, this.SanitizedFieldTitle,
                    MaskField(this.SanitizedFieldTitle),
                    this.CurrentFieldTitle, CreateDefaultValue()));
            }
        }
        else
        {
            OnInitial?.Invoke(this, new FieldPropertiesInteraction(
                interaction, this.SanitizedFieldTitle,
                csrfTokenSourceInteraction?.Fields.PackField(formName, this.SanitizedFieldTitle) ??
                this.SanitizedFieldTitle,
                this.CurrentFieldTitle, ""));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}