namespace Ziewaar.RAD.Doodads.CommonComponents;

public class LatestTemplateUpdate() : ITaggedData<Stream>
{
    public SidechannelTag Tag { get; set; } = SidechannelTag.UpdateWhenChanged;
    public Stream Data { get; } = new MemoryStream();
}

public class TemplateParser(string placeholderStart = "{% ", string placeholderEnd = " %}")
{
    private readonly List<TemplateCommand> CommandStack = new();
    public IReadOnlyList<TemplateCommand> TemplateCommands => CommandStack;
    public ITaggedData<Stream> CurrentTemplateData { get; private set; } = new LatestTemplateUpdate();
    public bool RefreshTemplateData(ITaggedData<Stream> refreshedData)
    {
        if (refreshedData.Tag.Stamp == CurrentTemplateData.Tag.Stamp)
            return false;
        CommandStack.Clear();
        CurrentTemplateData = refreshedData;
        using var templateReader = new StreamReader(CurrentTemplateData.Data);
        var allTemplateText = templateReader.ReadToEnd();
        for(int selectionHead = 0;
            selectionHead < allTemplateText.Length;
            selectionHead = FindNextHead(allTemplateText, selectionHead))
            ;
        return true;
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