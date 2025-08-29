using HtmlAgilityPack;
using System.Reflection.Metadata;
using System.Security.Principal;
using Ziewaar.RAD.Doodads.FormsValidation.HTML;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;

public class ReadHtmlForm : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    private FormStructureInteraction? CurrentForm = null;
    private TextSinkingInteraction? CurrentHtmlSink = null;
    private static readonly string[] InputElementNames = ["input", "select", "textarea", "button"];
    private static string InputElementFilter => string.Join('|', InputElementNames.Select(x => $".//{x}"));

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ISinkingInteraction>(out var output) || output == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "No sink found to template into."));
            return;
        }

        if (CurrentForm != null && CurrentHtmlSink != null)
        {
            var probeForUpdate = new CheckUpdateRequiredInteraction(interaction, CurrentHtmlSink);
            OnThen?.Invoke(this, probeForUpdate);
            if (probeForUpdate.IsRequired)
            {
                CurrentHtmlSink?.SinkBuffer.Dispose();
                CurrentHtmlSink = null;
                CurrentForm = null;
            }
        }

        if (CurrentForm == null)
        {
            CurrentHtmlSink = TextSinkingInteraction.CreateIntermediateFor(output, interaction);
            OnElse?.Invoke(this, CurrentHtmlSink);
            HtmlDocument fullHtmlDocument = new();
            CurrentHtmlSink.SinkBuffer.Position = 0;
            fullHtmlDocument.Load(CurrentHtmlSink.SinkBuffer, CurrentHtmlSink.TextEncoding);

            if (fullHtmlDocument.DocumentNode.SelectSingleNode("//form") is not HtmlNode formNode)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "missing form node"));
                return;
            }

            var structureBuilder = FormStructureInteraction.Builder
                .WithContentType(formNode.GetAttributeValue("enctype", "application/x-www-form-urlencoded"))
                .WithMethod(formNode.GetAttributeValue("method", "GET"))
                .WithAction(formNode.GetAttributeValue("action", ""));

            var inputGroups = formNode.SelectNodes(InputElementFilter)
                .Select(inputNode => (inputName: inputNode.GetAttributeValue("name", string.Empty), node: inputNode))
                .Where(tuple => !string.IsNullOrWhiteSpace(tuple.inputName))
                .GroupBy(x => x.inputName, x => x.node);

            foreach (IGrouping<string, HtmlNode> inputGroup in inputGroups)
            {
                var inputBuilder = FormStructureMember.Builder;

                var inputTypes = inputGroup.Select(x => (tag: x.Name, type: x.GetAttributeValue("type", "text")))
                    .Distinct().ToArray();

                if (inputTypes.Any(x => x.type == "file") && inputTypes.Any(x => x.type != "file"))
                {
                    OnException?.Invoke(this,
                        new CommonInteraction(interaction,
                            "Form declares a field that's both a file and a regular input"));
                    return;
                }

                var fieldtypeValidator = new AndValidator(
                        inputTypes.Select(field => new FieldTypeValidator(field.tag, field.type)).ToArray<IPrioritizedValidator>());

                var validTopLevelOptions = inputGroup
                    .Select(groupedInputNode => groupedInputNode.GetAttributeOrDefault("value")).OfType<string>()
                    .Distinct();
                var validSelectOptions = inputGroup.SelectMany(subNodes =>
                        subNodes.ChildNodes.Where(optionCandidate => optionCandidate.Name == "option"))
                    .Select(x => x.GetAttributeOrDefault("value")).OfType<string>().Distinct();

                var validOptions = validTopLevelOptions.Concat(validSelectOptions).ToArray();
                var optionsValidator = new OptionsValidator(validOptions);

                var minLength = inputGroup.Select(groupedInputNode =>
                    groupedInputNode.GetUnsignedAttributeOrDefault("minlength", uint.MinValue)).Max() ?? uint.MinValue;
                var maxLength = inputGroup.Select(groupedInputNode =>
                    groupedInputNode.GetUnsignedAttributeOrDefault("maxlength", uint.MaxValue)).Min() ?? uint.MaxValue;

                var lengthValidator = new LengthValidator(minLength, maxLength);
                
                var minConstraints = inputGroup
                    .Select(groupedInputNode => groupedInputNode.GetAttributeOrDefault("min")).OfType<string>()
                    .Distinct().ToArray();
                var maxConstraints = inputGroup
                    .Select(groupedInputNode => groupedInputNode.GetAttributeOrDefault("max")).OfType<string>()
                    .Distinct().ToArray();
                var patternConstraints = inputGroup
                    .Select(groupedInputNode => groupedInputNode.GetAttributeOrDefault("pattern")).OfType<string>()
                    .Distinct().ToArray();

                var lowerValueCountLimit = inputGroup.Select(groupedInputNode =>
                    groupedInputNode.GetNumericAttributeOrDefault("minlength", Decimal.MinValue) > 0 ||
                    groupedInputNode.Attributes.Any(x => x.Name == "required")).Count();

                var selects = inputGroup.Where(selectCandidate => selectCandidate.Name == "select").ToArray();
                var nonSelectCount = inputGroup.Count(selectCandidate => selectCandidate.Name != "select");
                var singleSelectCount = selects.Count(selectNode =>
                    selectNode.Attributes.All(singleCandidate => singleCandidate.Name != "multiple"));
                var multiSelectCount = selects.Where(selectNode =>
                        selectNode.Attributes.Any(multipleCandidate => multipleCandidate.Name == "multiple"))
                    .Select(multiSelectMembers =>
                        multiSelectMembers.ChildNodes.Count(candidateOption => candidateOption.Name == "option")).Sum();

                var upperValueCountLimit = nonSelectCount + singleSelectCount + multiSelectCount;
                var allDisabledOrReadonly = inputGroup.All(inputField =>
                    inputField.Attributes.Any(disabledReadonlyCandidate =>
                        disabledReadonlyCandidate.Name is "disabled" or "readonly"));
            }
        }

        OnThen?.Invoke(this, CurrentForm);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class LengthValidator(uint minLength, uint maxLength) : IPrioritizedValidator
{
    public bool TryValidate(IEnumerable value, [NotNullWhen(true)] out object? validOutput)
    {
        var allStrings = value.OfType<object>().Select(x => x.ToString()).ToArray();
        bool satisfied = allStrings.All(x => x != null);
        validOutput = value;
        
        if (minLength > 0)
            satisfied &= allStrings.OfType<string>().All(serializedValue => serializedValue.Length >= minLength);
        
        if (maxLength < uint.MaxValue) 
            satisfied &= allStrings.OfType<string>().All(serializedValue => serializedValue.Length <= maxLength);

        return satisfied;
    }
}

public class FieldTypeValidator(string fieldTag, string fieldType) : IPrioritizedValidator
{
    public bool TryValidate(IEnumerable value, [NotNullWhen(true)] out object? validOutput)
    {
        validOutput = value;
        if (fieldType == "file")
        {
            return value.OfType<object>().All(x => x is Stream);
        }

        if (fieldTag == "input")
        {
            var inputStrings = value.OfType<object>().Select(x => x.ToString()).OfType<string>();
            switch (fieldType)
            {
                case "color":
                    return inputStrings.All(fieldValue =>
                        fieldValue.StartsWith('#') && fieldValue.Length is 4 or 7);
                case "date":
                    var dateOnlyValidation = inputStrings.Select(fieldValue =>
                        (valid: DateOnly.TryParse(fieldValue, out var parsedValue), value: parsedValue)).ToArray();
                    validOutput = dateOnlyValidation.Select(x => x.value).ToArray();
                    return dateOnlyValidation.All(x => x.valid);
                case "datetime-local":
                    var dateTimeValidation = inputStrings.Select(fieldValue =>
                        (valid: DateTime.TryParse(fieldValue, out var parsedValue), value: parsedValue)).ToArray();
                    validOutput = dateTimeValidation.Select(x => x.value).ToArray();
                    return dateTimeValidation.All(x => x.valid);
                case "email":
                    return inputStrings.All(x => EmailValidation.EmailValidator.Validate(x));
                case "month":
                    var monthValidation = inputStrings.Select(fieldValue =>
                            (valid: DateOnly.TryParse(fieldValue + "-01", out var parsedValue), value: parsedValue))
                        .ToArray();
                    validOutput = monthValidation.Select(x => x.value).ToArray();
                    return monthValidation.All(x => x.valid);
                case "number":
                case "range":
                    var numberValidation = inputStrings.Select(fieldValue =>
                        (valid: Decimal.TryParse(fieldValue, out var parsedValue), value: parsedValue)).ToArray();
                    validOutput = numberValidation.Select(x => x.value).ToArray();
                    return numberValidation.All(x => x.valid);
                case "time":
                    var timeValidation = inputStrings.Select(fieldValue =>
                        (valid: TimeOnly.TryParse(fieldValue, out var parsedValue), value: parsedValue)).ToArray();
                    validOutput = timeValidation.Select(x => x.value).ToArray();
                    return timeValidation.All(x => x.valid);
                case "week":

                    var weekValidation = inputStrings.Select(fieldValue =>
                        (valid: WeekOnly.TryParse(fieldValue, out var parsedValue), value: parsedValue)).ToArray();
                    validOutput = weekValidation.Select(x => x.value?.ToDateOnly()).ToArray();
                    return weekValidation.All(x => x.valid);
            }
        }

        return true;
    }
}

public class WeekOnly(int year, int week)
{
    public static bool TryParse(string fieldValue, [NotNullWhen(true)] out WeekOnly? result)
    {
        result = null;
        if (fieldValue.Length != "1970-W1".Length && fieldValue.Length != "1970-W11".Length)
            return false;
        var splitPos = fieldValue.IndexOf("-W", StringComparison.Ordinal);
        if (splitPos != 4)
            return false;
        var year = fieldValue.Substring(0, 4);
        var week = fieldValue.Substring(6);
        if (year.All(char.IsAsciiDigit) && week.All(char.IsAsciiDigit) && week.Length is < 3 and > 0 && 
            int.TryParse(year, out var yearValue) && int.TryParse(week, out var weekValue))
        {
            result = new(year: yearValue, week: weekValue);
            return true;
        }
        else
        {
            return false;
        }
    }

    public DateOnly ToDateOnly()
    {
        var jan4 = new DateTime(year, 1, 4); // always in week 1
        var startOfWeek1 = jan4.AddDays(-(int)jan4.DayOfWeek + 1); // Monday of week 1
        var weekStart = startOfWeek1.AddDays((week - 1) * 7);
        return DateOnly.FromDateTime(weekStart);
    }
}

public interface IPrioritizedValidator
{
    bool TryValidate(IEnumerable value, [NotNullWhen(true)] out object? validOutput);
}

public class OrValidator(params IPrioritizedValidator[] validators) : IPrioritizedValidator
{
    public bool TryValidate(IEnumerable value, [NotNullWhen(true)] out object? validOutput)
    {
        foreach (IPrioritizedValidator validator in validators)
            if (validator.TryValidate(value, out validOutput))
                return true;
        validOutput = null;
        return false;
    }
}

public class AndValidator(params IPrioritizedValidator[] precedents) : IPrioritizedValidator
{
    public bool TryValidate(IEnumerable value, [NotNullWhen(true)] out object? validOutput)
    {
        validOutput = null;
        foreach (IPrioritizedValidator validator in precedents)
            if (!validator.TryValidate(value, out validOutput))
                return false;
        return true;
    }
}

public class OptionsValidator(string[] validOptions) : IPrioritizedValidator
{
    public bool TryValidate(IEnumerable value, [NotNullWhen(true)] out object? validOutput)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        if (validOptions.Any() && value.OfType<Stream>().Any())
        {
            validOutput = null;
            return false;
        }

        // ReSharper disable once PossibleMultipleEnumeration
        var serializedValues = value.OfType<object>().Select(x => x.ToString());
        if (serializedValues.All(validOptions.Contains))
        {
            validOutput = serializedValues;
            return true;
        }
        else
        {
            validOutput = null;
            return false;
        }
    }
}

public static class NodeExtensions
{
    public static string? GetAttributeOrDefault(this HtmlNode node, string name) =>
        node.GetAttributes().SingleOrDefault(x => x.Name == name)?.Value;

    public static decimal? GetNumericAttributeOrDefault(this HtmlNode node, string name, decimal defaultValue) =>
        decimal.TryParse(node.GetAttributeOrDefault(name), out decimal result) ? result : defaultValue;
    public static uint? GetUnsignedAttributeOrDefault(this HtmlNode node, string name, uint defaultValue) =>
        uint.TryParse(node.GetAttributeOrDefault(name), out uint result) ? result : defaultValue;
}

public class FormStructureInteractionBuilder
{
    private string ContentType = "application/x-www-form-urlencoded";
    private HttpMethod HttpMethod = HttpMethod.Get;
    private string ActionUrl = "";

    public FormStructureInteractionBuilder WithContentType(string contentType)
    {
        this.ContentType = contentType;
        return this;
    }

    public FormStructureInteractionBuilder WithMethod(string method)
    {
        this.HttpMethod = HttpMethod.Parse(method);
        return this;
    }

    public FormStructureInteractionBuilder WithAction(string actionUrl)
    {
        this.ActionUrl = actionUrl;
        return this;
    }
}

public class FormStructureMemberBuilder
{
}

public class FormStructureMember
{
    public static FormStructureMemberBuilder Builder => new();
}

public class FormStructureInteraction : IInteraction
{
    public IInteraction Stack { get; }
    public object Register { get; }
    public IReadOnlyDictionary<string, object> Memory { get; }
    public static FormStructureInteractionBuilder Builder => new();
}