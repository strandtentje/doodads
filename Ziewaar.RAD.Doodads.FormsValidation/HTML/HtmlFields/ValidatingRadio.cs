using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ValidatingRadio(bool isRequired, string name, List<string> validValues) : IValidatingInputFieldInSet
{
    public bool IsRequired => isRequired;
    public bool IsMaxUnbound => false;
    public int MinExpectedValues
    {
        get => isRequired ? 1 : 0;
        set { }
    }
    public int MaxExpectedValues
    {
        get => 1;
        set { }
    }
    public string Name => name;
    private IEnumerable<string> ValidValues => validValues;
    public List<IValidatingInputField> AltValidators { get; } = new();
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "radio")
            return false;
        if (node.GetInputName() is not string inputName)
            return true;
        if (node.GetInputValue() is string radioValue)
            set.Merge(new ValidatingRadio(node.IsRequired(), inputName, [radioValue]));
        return true;
    }
    public bool TryValidate(string[] submittedValue, out object? result)
    {
        if (submittedValue.Length == 1 && validValues.Contains(submittedValue.Single()) ||
            submittedValue.Length == 0 && !isRequired)
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
        if (otherFieldInSet is ValidatingRadio otherRadio)
        {
            validValues.AddRange(otherRadio.ValidValues);
            return true;
        }
        return false;
    }
}