using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ValidatingDatePicker : IValidatingInputFieldInSet
{
    public int MinExpectedValues { get; set; }
    public int MaxExpectedValues { get; set; }
    public bool IsMaxUnbound => false;
    private DateOnly Min, Max;
    public string Name { get; private set; }
    public List<IValidatingInputField> AltValidators { get; } = new();
    public bool IsRequired { get; private set; }
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "date")
            return false;
        if (node.GetInputName() is not string inputName)
            return true;

        var minValue = DateOnly.TryParse(node.GetMin() ?? DateOnly.MinValue.ToString(), out var candidateMinValue)
            ? candidateMinValue
            : DateOnly.MinValue;
        var maxValue = DateOnly.TryParse(node.GetMax() ?? DateOnly.MaxValue.ToString(), out var candidateMaxValue)
            ? candidateMaxValue
            : DateOnly.MaxValue;
        var isRequired = node.IsRequired();
        set.Merge(new ValidatingDatePicker()
        {
            Name = inputName, IsRequired = isRequired, Min = minValue, Max = maxValue
        });
        return true;
    }
    private (bool wasParsed, DateOnly value) Parse(string dateText) =>
        (DateOnly.TryParse(dateText, out var only), only);
    public bool TryValidate(string[] submittedValue, out object? result)
    {
        var readDates = submittedValue.Select(Parse).TakeWhile(x => x.wasParsed).Select(x => x.value)
            .Where(x => Min <= x && Max >= x).ToArray();
        result = readDates;
        return readDates.Length == submittedValue.Length;
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        if (otherFieldInSet is ValidatingDatePicker otherDatePicker)
        {
            Min = otherDatePicker.Min > Min ? otherDatePicker.Min : Min;
            Max = otherDatePicker.Max < Max ? otherDatePicker.Max : Max;
            return true;
        }
        else
        {
            return false;
        }
    }
}