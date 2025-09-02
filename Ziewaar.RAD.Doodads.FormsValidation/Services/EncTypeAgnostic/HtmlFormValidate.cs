using System.Diagnostics;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class HtmlFormValidate : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<FormStructureInteraction>(out var formStructure) || formStructure == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Form structure required for this; use ie. HtmlFormPrepare"));
            return;
        }

        if (!interaction.TryGetClosest<IFieldNameMappingInteraction>(out var nameMapping) || nameMapping == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Name mapping required"));
            return;
        }

        if (!interaction.TryGetClosest<FormDataInteraction>(out var formData) || formData == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Form data required for this."));
            return;
        }

        var validationBuilder = FormValidationInteraction.Build().SetStack(interaction);
        List<FormStructureMember> remainingFields = formStructure.Members.ToList();
        foreach (var field in formData.Data)
        {
            if (!nameMapping.TryGetRealNameOf(field.Key, out string? realName) || realName == null)
            {
                validationBuilder.SetHeadFailure("Unknown field name in data");
                break;
            }

            if (realName == "choice" || realName == "color")
            {
                Debug.WriteLine("hi");
            }

            var memberIndex =
                remainingFields.FindIndex(x => x.Name.Equals(realName, StringComparison.OrdinalIgnoreCase));
            var validationMember = remainingFields[memberIndex];
            remainingFields.RemoveAt(memberIndex);

            if (!TryAmendValidationBuilder(validationBuilder, validationMember, field))
                break;
        }

        foreach (FormStructureMember leftover in remainingFields)
            if (!TryAmendValidationBuilder(validationBuilder, leftover, []))
                break;
        
        var validation = validationBuilder.Build();
        if (validation.IsValid)
            OnThen?.Invoke(this, validation);
        else
            OnElse?.Invoke(this, validation);
    }

    private static bool TryAmendValidationBuilder(
        FormValidationInteraction.FormValidationInteractionBuilder validationBuilder,
        FormStructureMember validationMember, IEnumerable<object> fieldValues)
    {
        var validatingCollection = validationMember.CreateValidatingCollection();

        foreach (object incomingData in fieldValues)
            validatingCollection.Add(incomingData, out var _);
        if (validatingCollection.IsSatisfied)
        {
            var validatedValues = validatingCollection.ValidItems.OfType<object>().ToArray();
            if (validatedValues.Length == 0)
                validationBuilder.NoValuesFor(validationMember.Name);
            else if (validatedValues.Length == 1)
                validationBuilder.SingleValueFor(validationMember.Name, validatedValues.Single());
            else
                validationBuilder.MultipleValuesFor(validationMember.Name, validatedValues);
        }
        else
        {
            validationBuilder.SetFieldFailure(validationMember.Name);
            return false;
        }

        return true;
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}