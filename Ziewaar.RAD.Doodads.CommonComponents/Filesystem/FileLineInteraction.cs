#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

public class FileLineInteraction : IInteraction
{
    private readonly IInteraction interaction;

    public FileLineInteraction(IInteraction interaction, string lineName)
    {
        this.interaction = interaction;
        Memory = new SwitchingDictionary(
        [lineName], key => key == lineName ? LineNumber : throw new KeyNotFoundException());
    }

    public IInteraction Stack => interaction;
    public object Register { get; set; }
    public int LineNumber { get; set; }
    public string? ChangedLine, LineBefore, LineAfter;
    public bool SkipLine = false;
    public bool IsChanged => ChangedLine != null || LineBefore != null || LineAfter != null || SkipLine;
    public void Reset()
    {
        SkipLine = false;
        ChangedLine = null;
        LineBefore = null;
        LineAfter = null;
    }
    public IReadOnlyDictionary<string, object> Memory { get; }
}