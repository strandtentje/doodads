using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ValidatingTimePicker(HtmlNode node) : IValidatingInputFieldInSet
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
    public int MinExpectedValues { get; set; }
    public int MaxExpectedValues { get; set; }
    public bool IsMaxUnbound => false;
    private TimeOnly Min, Max;
    public List<IValidatingInputField> AltValidators { get; } = new();
    public bool IsRequired { get; private set; }
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "time")
            return false;
        if (node.GetInputName() is not string inputName)
            return true;

        var minValue = TimeOnly.TryParse(node.GetMin() ?? TimeOnly.MinValue.ToString(), out var candiTimeMinValue)
            ? candiTimeMinValue
            : TimeOnly.MinValue;
        var maxValue = TimeOnly.TryParse(node.GetMax() ?? TimeOnly.MaxValue.ToString(), out var candiTimeMaxValue)
            ? candiTimeMaxValue
            : TimeOnly.MaxValue;
        var isRequired = node.IsRequired();
        set.Merge(new ValidatingTimePicker(node)
        {
            IsRequired = isRequired, Min = minValue, Max = maxValue
        });
        return true;
    }
    private (bool wasParsed, TimeOnly value) Parse(string timeText) =>
        (TimeOnly.TryParse(timeText, out var only), only);
    public bool TryValidate(string[] submittedValue, out IEnumerable result)
    {
        var readTimes = submittedValue.Select(Parse).TakeWhile(x => x.wasParsed).Select(x => x.value)
            .Where(x => Min <= x && Max >= x).ToArray();
        result = readTimes;
        return readTimes.Length == submittedValue.Length;
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        if (otherFieldInSet is ValidatingTimePicker otherTimePicker)
        {
            Min = otherTimePicker.Min > Min ? otherTimePicker.Min : Min;
            Max = otherTimePicker.Max < Max ? otherTimePicker.Max : Max;
            return true;
        }
        else
        {
            return false;
        }
    }
}