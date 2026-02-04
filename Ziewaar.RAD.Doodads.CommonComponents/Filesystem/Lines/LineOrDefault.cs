#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Lines;

[Category("System & IO")]
[Title("Read Single Line from File or Default")]
[Description("Reads the first line from a text file into register, or puts default into register")]
[ShortNames("lid")]
public class LineOrDefault : IService
{
    [PrimarySetting("Default Line Text")]
    private readonly UpdatingPrimaryValue DefaultLineConstant = new();
    private string? CurrentDefaultText;
    [EventOccasion("In case the file existed an a line was readable, line will be register here.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the file didn't exist or didn't contain a line")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When there wasn't a path in register")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, DefaultLineConstant).IsRereadRequired(out string? candidateRepeater))
            CurrentDefaultText = candidateRepeater;
        if (string.IsNullOrWhiteSpace(CurrentDefaultText) || CurrentDefaultText == null)
        {
            OnException?.Invoke(this, interaction.AppendRegister("Default text required"));
            return;
        }
        if (interaction.Register.ToString() is not string candidateFilename)
        {
            OnException?.Invoke(this, interaction.AppendRegister("Path expected in register"));
            return;
        }
        var info = new FileInfo(candidateFilename);
        if (!info.Exists)
        {
            OnElse?.Invoke(this, interaction.AppendRegister(CurrentDefaultText));
            return;
        }

        string? line = null;
        using (var reader = new StreamReader(info.FullName))
        {
            if (!reader.EndOfStream)
                line = reader.ReadLine();
        }

        if (line == null)
        {
            OnElse?.Invoke(this, interaction.AppendRegister(CurrentDefaultText));
        } else
        {
            OnThen?.Invoke(this, interaction.AppendRegister(line));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
