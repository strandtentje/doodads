namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

public static class TemplateFilterExtensions
{
    public static string ApplyFilterTo(this TemplateCommandType type, string rawText) =>
        (type | TemplateCommandType.AllFilters) switch
        {
            TemplateCommandType.HtmlFilter => HttpUtility.HtmlEncode(rawText),
            TemplateCommandType.UrlFilter => HttpUtility.UrlEncode(rawText),
            TemplateCommandType.AttributeFilter => HttpUtility.HtmlAttributeEncode(rawText),
            TemplateCommandType.JsFilter => HttpUtility.JavaScriptStringEncode(rawText),
            _ => rawText
        };
}