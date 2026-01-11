namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

public class PassthroughFilter : ITemplateFilter
{
    private PassthroughFilter(string cleanPayload, TemplateCommandType command) =>
        (CleanPayload, Command) = (cleanPayload, command);

    public string CleanPayload { get; }
    public TemplateCommandType Command { get; }

    public static bool TryParse(string dirtyPayload, out ITemplateFilter? result)
    {
        var cleanPayload = dirtyPayload.Trim().TrimTemplateTokens();
        var modifiers = dirtyPayload.Substring(0, dirtyPayload.Length - cleanPayload.Length);
        result = new PassthroughFilter(cleanPayload, modifiers.ConvertToCommandType());
        return true;
    }

    public string Render(object value) => Convert.ToString(value);
}