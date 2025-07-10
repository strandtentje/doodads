using HtmlAgilityPack;
using System.Web;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;

public class ValidatingInputFieldSet(HttpMethod method, string route) : IValidatingInputFieldSet
{
    private readonly List<IValidatingInputField> fieldIngestors = new();
    public HttpMethod Method => method;
    public IEnumerable<string> FieldNames => fieldIngestors.Select(x => x.Name);
    public override string ToString() => $"{method} {route}";
    public void ApplyObfuscation(ICsrfFields fields)
    {
        foreach (var item in fieldIngestors)
            item.Name = fields.NewObfuscation(this.ToString(), item.Name);
    }
    public void ParseAndMergeNode(HtmlNode node)
    {
        if (node.GetInputName() == null) return;
        if (ValidatingSelectbox.TryInsertInto(node, this) ||
            ValidatingTextInput.TryInsertInto(node, this) ||
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
    public static ValidatingInputFieldSet Parse(HtmlDocument document)
    {
        var formNode = document.DocumentNode.SelectSingleNode("//form");
        if (formNode == null)
            throw new FormValidationMarkupException($"No form specified");
        var methodFound = formNode.GetAttributeValue("method", "GET").ToUpperInvariant();
        if (methodFound != "POST" && methodFound != "GET")
            throw new FormValidationMarkupException($"Can only post and get");
        var actionFound = formNode.GetAttributeValue("action", "");
        if (string.IsNullOrWhiteSpace(actionFound))
            throw new FormValidationMarkupException($"Action required");
        var inputNodes = formNode.SelectNodes(".//input|.//select|.//textarea")?.ToArray() ?? [];
        var fieldset = new ValidatingInputFieldSet(HttpMethod.Parse(methodFound), actionFound);
        foreach (var inputElement in inputNodes)
            fieldset.ParseAndMergeNode(inputElement);
        foreach (var item in fieldset.fieldIngestors)
        {
            item.NameChanged += (s, e) =>
            {
                formNode.ChangeLabelNames(e.oldName, e.newName);
            };
        }
        return fieldset;
    }
    public void Merge(IValidatingInputFieldInSet fieldInSet)
    {
        if (fieldIngestors.SingleOrDefault(x => x.Name == fieldInSet.Name) is not IValidatingInputField existingField)
        {
            fieldInSet.MinExpectedValues = fieldInSet.IsRequired ? 1 : 0;
            fieldInSet.MaxExpectedValues = fieldInSet.IsMaxUnbound ? short.MaxValue : 1;
            fieldIngestors.Add(fieldInSet);
        }
        else if (existingField.TryIdentityMerge(fieldInSet))
        {
            existingField.MinExpectedValues += fieldInSet.MinExpectedValues;
            existingField.MaxExpectedValues += fieldInSet.MaxExpectedValues;
        }
        else
        {
            existingField.AltValidators.Add(fieldInSet);
            existingField.MinExpectedValues += fieldInSet.MinExpectedValues;
            existingField.MaxExpectedValues += fieldInSet.MaxExpectedValues;
        }
    }
    public bool IsValidationRequired(string requestMethod, string requestUrl) =>
        HttpMethod.Parse(requestMethod) == method && HttpUtility.UrlDecode(requestUrl) == route;
    public ValidationResult[] Validate(IReadOnlyDictionary<string, IEnumerable> inputValues)
    {
        var result = new ValidationResult[fieldIngestors.Count];
        for (int i = 0; i < fieldIngestors.Count; i++)
        {
            IValidatingInputField? validator = fieldIngestors[i];
            if (!inputValues.TryGetValue(validator.Name, out var valueToValidate)
                || valueToValidate is not IEnumerable<string> stringsToValidate)
            {
                result[i] = new(validator.Name, Enumerable.Empty<object>(), validator.MinExpectedValues > 0);
            }
            else if (
                stringsToValidate.Count() < validator.MinExpectedValues ||
                stringsToValidate.Count() > validator.MaxExpectedValues)
            {
                result[i] = new(validator.Name, Enumerable.Empty<object>(), true);
            }
            else if (validator.TryValidate(stringsToValidate.ToArray(), out var resultValues))
            {
                var resultArray = resultValues.OfType<object>().ToArray();
                if (resultArray.Length < validator.MinExpectedValues ||
                    resultArray.Length > validator.MaxExpectedValues)
                {
                    result[i] = new ValidationResult(validator.Name, Enumerable.Empty<object>(), true);
                }
                else
                {
                    result[i] = new ValidationResult(validator.Name, resultArray, false);
                }
            }
            else if (validator.AltValidators != null)
            {
                foreach (var alt in validator.AltValidators)
                {
                    if (alt.TryValidate(stringsToValidate.ToArray(), out var altOutput))
                    {
                        var resultArray = altOutput.OfType<object>().ToArray();
                        if (resultArray.Length >= validator.MinExpectedValues ||
                            resultArray.Length <= validator.MaxExpectedValues)
                        {
                            result[i] = new ValidationResult(validator.Name, resultArray, false);
                            break;
                        }
                    }
                }
                result[i] ??= new ValidationResult(validator.Name, Enumerable.Empty<object>(), true);
            }
            else
            {
                result[i] ??= new ValidationResult(validator.Name, Enumerable.Empty<object>(), true);
            }
        }
        return result;
    }
}

public record ValidationResult(string name, object value, bool isError);