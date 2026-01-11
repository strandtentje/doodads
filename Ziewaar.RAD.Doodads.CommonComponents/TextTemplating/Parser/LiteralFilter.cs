namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

public class LiteralFilter : ITemplateFilter
{
    private LiteralFilter(string cleanPayload, TemplateCommandType command) =>
        (CleanPayload, Command) = (cleanPayload, command);

    public static bool TryParse(string dirtyPayload, out ITemplateFilter? result)
    {
        var cleanPayload = dirtyPayload.Trim().TrimTemplateTokens();
        var modifiers = dirtyPayload.Substring(0, dirtyPayload.Length - cleanPayload.Length);

        var trimmedClean = cleanPayload.Trim();
        result = null;
        if (!trimmedClean.StartsWith("\"") || !trimmedClean.EndsWith("\""))
            return false;

        var literalClean = trimmedClean.Trim('"');

        result = new LiteralFilter(literalClean, modifiers.ConvertToCommandType());

        return true;
    }

    public string CleanPayload { get; }
    public TemplateCommandType Command { get; }
    public string Render(object value) => CleanPayload;
}