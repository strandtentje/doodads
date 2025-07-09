using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public static class HtmlFormExtensions
{
    public static string GetInputTypeName(this HtmlNode node) =>
        node.Attributes.FirstOrDefault(x => x.Name == "type")?.Value ?? "text";
    public static string? GetInputName(this HtmlNode node) =>
        node.Attributes.FirstOrDefault(x => x.Name == "name")?.Value;
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
}