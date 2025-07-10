using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ValidatingTextInput(HtmlNode node) : IValidatingInputFieldInSet
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
    public int MinLength, MaxLength;
    public Regex Pattern = new(".*");
    public int MinExpectedValues { get; set; }
    public int MaxExpectedValues { get; set; }
    public List<IValidatingInputField> AltValidators { get; } = new();
    public bool IsRequired { get; private set; }
    public bool IsMaxUnbound => false;
    public bool TryValidate(string[] submittedValue, out IEnumerable result)
    {
        var readPw = submittedValue.Where(x => x.Length >= MinLength && x.Length <= MaxLength && Pattern.IsMatch(x)).ToArray();
        result = readPw;
        return readPw.Length == submittedValue.Length;
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        if (otherFieldInSet is ValidatingTextInput otherNumPicker)
        {
            MinLength = otherNumPicker.MinLength > MinLength ? otherNumPicker.MinLength : MinLength;
            MaxLength = otherNumPicker.MaxLength < MaxLength ? otherNumPicker.MaxLength : MaxLength;
            return true;
        }
        else
        {
            return false;
        }
    }
    private static readonly string[] FieldTypes = ["password", "text", "search", "tel", "text", "url"];
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        var nodeType = node.GetInputTypeName();
        if (!FieldTypes.Any(x => string.Compare(x, nodeType, true) == 0) && node.Name != "textarea")
            return false;
        if (node.GetInputName() is not string inputName)
            return true;
        var isRequired = node.IsRequired();
        set.Merge(new ValidatingTextInput(node)
        {
            IsRequired = isRequired,
            MinLength = node.GetMinLength(),
            MaxLength = node.GetMaxLength(),
            Pattern = node.GetPattern()
        });
        return true;
    }
}