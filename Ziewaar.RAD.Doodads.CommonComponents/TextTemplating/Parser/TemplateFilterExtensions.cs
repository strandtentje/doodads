using Ziewaar.RAD.Doodads.CommonComponents.Transform;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

public static class TemplateFilterExtensions
{
    public static string ApplyFilterTo(this TemplateCommandType type, string rawText)
    {
        if (type.HasFlag(TemplateCommandType.SnakeCaseFilter))
            rawText = rawText.SnakeCase();
        if (type.HasFlag(TemplateCommandType.UrlFilter))
            rawText = HttpUtility.UrlEncode(rawText);
        if (type.HasFlag(TemplateCommandType.UrlDataFilter))
            rawText = Uri.EscapeDataString(rawText).Replace("#", "%23");
        if (type.HasFlag(TemplateCommandType.AttributeFilter))
            rawText = HttpUtility.HtmlAttributeEncode(rawText);
        if (type.HasFlag(TemplateCommandType.HtmlFilter))
            rawText = HttpUtility.HtmlEncode(rawText);
        if (type.HasFlag(TemplateCommandType.JsFilter))
            rawText = HttpUtility.JavaScriptStringEncode(rawText);
        return rawText;
    }

    public static string SnakeCase(this string txt) => string.Join("_", txt.AlphanumericParts);

    public static IEnumerable<string> AlphanumericParts(this string txt)
    {
        for (int cursor = 0; cursor < txt.Length; cursor++)
        {
            var startingCursor = cursor;
            for (; cursor < txt.Length && char.IsLetterOrDigit(txt[cursor]); cursor++) ;
            if (startingCursor > 0 || cursor < txt.Length)
                yield return txt.Substring(startingCursor, cursor - startingCursor);
            else
                yield return txt;

            if (cursor != txt.Length)
            {
                for (; cursor < txt.Length && !char.IsLetterOrDigit(txt[cursor]); cursor++) ;
                if (cursor == txt.Length)
                    yield return string.Empty;
            }
        }
    }
}