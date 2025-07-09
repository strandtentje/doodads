using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class FormValidationMarkupException(string message) : Exception(message);

public class ValidatingInputFieldSet : IValidatingInputFieldSet
{
    public void AddNode(HtmlNode node)
    {
        if (ValidatingTextInput.TryInsertInto(node, this) ||
            ValidatingTimePicker.TryInsertInto(node, this) ||
            ValidatingDatePicker.TryInsertInto(node, this) ||
            ValidatingWeekPicker.TryInsertInto(node, this) ||
            ValidatingMonthPicker.TryInsertInto(node, this) ||
            ValidatingDateTimeLocalPicker.TryInsertInto(node, this) ||
            ValidatingNumberPicker.TryInsertInto(node, this) ||
            ValidatingEmailPicker.TryInsertInto(node, this) ||
            ValidatingCheckbox.TryInsertInto(node, this) ||
            ValidatingColorPicker.TryInsertInto(node, this) ||
            ValidatingRadio.TryInsertInto(node, this))
        {
            return;
        }
        throw new FormValidationMarkupException($"Unknown field type seen {node.GetInputTypeName()}");
    }
    public void Merge(IValidatingInputFieldInSet fieldInSet)
    {
        throw new NotImplementedException();
    }
}