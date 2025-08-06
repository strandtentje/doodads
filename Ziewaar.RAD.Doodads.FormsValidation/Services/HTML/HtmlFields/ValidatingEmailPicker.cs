using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ValidatingEmailPicker(HtmlNode node) : IValidatingInputFieldInSet
{
    public event EventHandler<(string oldName, string newName)>? NameChanged;
    public string Name
    {
        get => node.GetInputName() ?? "";
        set
        {
            var oldName = node.GetInputName();
            node.SetInputName(value);
            if (oldName == null) return;
            NameChanged?.Invoke(this, (oldName, value));
        }
    }
    public Regex Pattern;
    public int MinExpectedValues { get; set; }
    public int MaxExpectedValues { get; set; }
    public bool IsMaxUnbound { get; private set; }
    public int MaxLength { get; private set; }
    public int MinLength { get; private set; }    
    public List<IValidatingInputField> AltValidators { get; } = new();
    public bool IsRequired { get; private set; }
    public bool TryValidate(string[] submittedValue, out IEnumerable result)
    {
        if (IsMaxUnbound)
            submittedValue = submittedValue.SelectMany(x => x.Split(',')).ToArray();
        result = submittedValue;
        return submittedValue.All(x =>
            x.Length <= MaxLength && 
            x.Length >= MinLength &&
            EmailValidation.EmailValidator.Validate(x) &&
            Pattern.IsMatch(x));
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        return otherFieldInSet is ValidatingEmailPicker otherMailPicker;
    }
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "email")
            return false;
        if (node.GetInputName() is not string inputName)
            return true;
        var isRequired = node.IsRequired();
        set.Merge(new ValidatingEmailPicker(node)
        {
            IsRequired = isRequired, MinLength = node.GetMinLength(), MaxLength = node.GetMaxLength(),
            IsMaxUnbound = node.IsMultiple(), Pattern = node.GetPattern()
        });
        return true;
    }
}