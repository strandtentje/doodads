namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

public interface ITemplateFilter
{
    string CleanPayload { get; }
    TemplateCommandType Command { get; }
    string Render(object value);
}