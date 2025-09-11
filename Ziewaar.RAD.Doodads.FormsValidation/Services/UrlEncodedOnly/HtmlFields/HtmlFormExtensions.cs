namespace Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly.HtmlFields;
public static class HtmlFormExtensions
{
    public static string GetInputTypeName(this HtmlNode node)
    {
        var r = node.Attributes.FirstOrDefault(x => x.Name == "type")?.Value ?? "text";
        return r;
    }

    public static string? GetInputName(this HtmlNode node) =>
        node.Attributes.FirstOrDefault(x => x.Name == "name")?.Value;
    public static void SetInputName(this HtmlNode node, string inputName)
    {
        if (node.Attributes.FirstOrDefault(x => x.Name == "name") is { } nameAttr)
        {
            nameAttr.Value = inputName;
        }
    }
    public static void ChangeLabelNames(this HtmlNode node, string oldName, string newName)
    {
        var matchingForNodes = node.
            SelectNodes("//label")?.
            Where(x => x.GetAttributes("for").Any(x => x.Value == oldName)) ?? [];
        foreach (var matchingLabels in matchingForNodes)
        {
            foreach (var matchingFors in matchingLabels.GetAttributes("for"))
            {
                matchingFors.Value = newName;
            }
        }
    }
    public static string GetRequiredInputName(this HtmlNode node) => node.GetInputName() ??
                                                                     throw new FormValidationMarkupException(
                                                                         "node must have a name");
    public static string? GetInputValue(this HtmlNode node) =>
        node.Attributes.FirstOrDefault(x => x.Name == "value")?.Value;
    public static string? GetMin(this HtmlNode node) =>
        node.Attributes.FirstOrDefault(x => x.Name == "min")?.Value;
    public static string? GetMax(this HtmlNode node) =>
        node.Attributes.FirstOrDefault(x => x.Name == "max")?.Value;
    public static int GetMinLength(this HtmlNode node) =>
        Convert.ToInt32(node.Attributes.FirstOrDefault(x => x.Name == "minlength")?.Value ?? "0");
    public static int GetMaxLength(this HtmlNode node) =>
        Convert.ToInt32(node.Attributes.FirstOrDefault(x => x.Name == "maxlength")?.Value ?? "0");
    public static Regex GetPattern(this HtmlNode node) =>
        new($"^{node.Attributes.FirstOrDefault(x => x.Name == "pattern")?.Value ?? ".*"}$");
    public static bool IsRequired(this HtmlNode node) =>
        node.Attributes.FirstOrDefault(x => x.Name == "required") != null;
    public static bool IsMultiple(this HtmlNode node) =>
        node.Attributes.FirstOrDefault(x => x.Name == "multiple") != null;

    public static void SetErrorSpan(this HtmlDocument document, string fieldName)
    {
        var matchingErrorNodes = document.DocumentNode.
            SelectNodes("//span")?.
            Where(x => x.GetAttributes("class").
            Any(x => x.Value.Contains($"error_{fieldName}"))) ?? [];
        foreach (var errorNode in matchingErrorNodes)
        {
            var classAttributes = errorNode.GetAttributes("class");
            foreach (var classAttribute in classAttributes)
            {
                classAttribute.Value = $"{classAttribute.Value.Replace("disabled", "")} enabled";
            }
        }
    }
}