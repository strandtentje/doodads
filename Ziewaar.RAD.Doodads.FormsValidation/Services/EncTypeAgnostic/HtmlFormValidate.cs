using System.Diagnostics;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

[Category("Input & Validation")]
[Title("For a prepared, applicable and optionally deobfuscated form, validate.")]
[Description("""
             Will validate incoming form data in a streaming way until validation breaks down.
             Consequentially, bad requests short circuit hard and early and the remaining request
             data will be rejected.
             """)]
public class HtmlFormValidate : IService
{
    [EventOccasion("""
                   When validation succeeded, this will output the field values under the names
                   as defined in the HTML form. When a field has multiple values, you will find
                   the value 'multiple' under the memory location with the fieldname suffixed with
                   ` state`. If there's nothing, it'll be 'empty'. If it's a single value that doesn't need
                   iterating, it'll be 'single'.
                   """)]
    public event CallForInteraction? OnThen;
    [EventOccasion("""
                   When validation fails, the first failed that it failed on will be marked in memory 
                   under its name suffixed with ` state` as 'failed'.
                   """)]
    public event CallForInteraction? OnElse;
    [EventOccasion("""
                   When there was no form data, no form was prepared or the names of the form fields couldn't 
                   be retrieved.
                   """)]
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

        using var validationBuilder = FormValidationInteraction.Build().SetStack(interaction);
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