namespace Ziewaar.RAD.Doodads.CommonComponents;

public class TemplateParser(string placeholderStart = "{% ", string placeholderEnd = " %}")
{
    private readonly List<TemplateCommand> CommandStack = new();
    public IReadOnlyList<TemplateCommand> TemplateCommands => CommandStack;
    public void RefreshTemplateData(StreamReader reader)
    {
        var allTemplateText = reader.ReadToEnd();
        for(int selectionHead = 0;
            selectionHead < allTemplateText.Length;
            selectionHead = FindNextHead(allTemplateText, selectionHead))
            ;
        reader.Dispose();
    }
    private int FindNextHead(string allTemplateText, int cursor)
    {
        var openerPosition = allTemplateText.OrdinalIndexOfPlaceholderStart(cursor);
        if (openerPosition < 0)
            openerPosition = allTemplateText.Length;
        CommandStack.Add(new()
        {
            Payload = allTemplateText.Substring(cursor, openerPosition - cursor),
            Type = TemplateCommandType.LiteralSource,
        });
        cursor = allTemplateText.OrdinalIndexOfPlaceholderEnd(openerPosition);
        if (cursor < 0)
            return allTemplateText.Length;

        var dirtyPayload = allTemplateText.Substring(
            openerPosition + placeholderStart.Length,
            cursor - openerPosition).Trim();
        var command = ParsePlaceholder(dirtyPayload);

        CommandStack.Add(command);
        return cursor + placeholderEnd.Length;
    }
    private static TemplateCommand ParsePlaceholder(string dirtyPayload)
    {
        var cleanPayload = dirtyPayload.TrimTemplateTokens();
        var modifiers = dirtyPayload.Substring(0, dirtyPayload.Length - cleanPayload.Length);
        TemplateCommand command = new()
        {
            Payload = cleanPayload,
            Type = modifiers.ElementAtOrDefault(0).ConvertToSourceCommandType() |
                   modifiers.ElementAtOrDefault(1).ConvertToFilterCommandType(),
        };
        return command;
    }
}