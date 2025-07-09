using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ValidatingCheckbox(bool isRequired, string name, List<string> validValues) : IValidatingInputFieldInSet
{
    public bool IsRequired => isRequired;
    public bool IsMaxUnbound => false;
    public int MinExpectedValues { get; set; }
    public int MaxExpectedValues { get; set; }
    public string Name => name;
    private IEnumerable<string> ValidValues => validValues;
    public List<IValidatingInputField> AltValidators { get; } = new();
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "checkbox")
            return false;
        if (node.GetInputName() is not string inputName)
            return true;
        if (node.GetInputValue() is string checkboxValue)
            set.Merge(new ValidatingCheckbox(node.IsRequired(), inputName, [checkboxValue]));
        return true;
    }
    public bool TryValidate(string[] submittedValue, out object? result)
    {
        if (submittedValue.All(validValues.Contains))
        {
            result = submittedValue;
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        if (otherFieldInSet is ValidatingCheckbox otherCheckbox)
        {
            validValues.AddRange(otherCheckbox.ValidValues);
            return true;
        }
        return false;
    }
}