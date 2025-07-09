using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ValidatingColorPicker : IValidatingInputFieldInSet
{
    public int MinExpectedValues { get; set; }
    public int MaxExpectedValues { get; set; }
    public bool IsMaxUnbound => false;
    public string Name { get; private set; }
    public List<IValidatingInputField> AltValidators { get; } = new();
    public bool IsRequired { get; private set; }
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "color")
            return false;
        if (node.GetInputName() is not string inputName)
            return true;
        var isRequired = node.IsRequired();
        set.Merge(new ValidatingColorPicker() { IsRequired = isRequired, Name = inputName, });
        return true;
    }
    private bool IsValidHexColor(string hexColor)
    {
        if (hexColor.Length != 7 && hexColor.Length != 4)
            return false;
        if (hexColor[0] != '#')
            return false;
        return hexColor.Skip(1).All(char.IsAsciiHexDigit);
    }
    public bool TryValidate(string[] submittedValue, out object? result)
    {
        result = submittedValue;
        return submittedValue.All(IsValidHexColor);
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        return otherFieldInSet is ValidatingColorPicker;
    }
}