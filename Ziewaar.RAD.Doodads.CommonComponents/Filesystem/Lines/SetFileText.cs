#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Lines;

[Category("System & IO")]
[Title("Set single text line into file")]
[Description("Overwrites contents of file with single line of text from variable. Attempts to create whole path.")]
public class SetFileText : IService
{
    [PrimarySetting("Line Text Variable")] private readonly UpdatingPrimaryValue LineVariableConstant = new();
    private string? CurrentLineVariableName;

    [EventOccasion("Sink file text here.")]
    public event CallForInteraction? OnThen;

    [NeverHappens] public event CallForInteraction? OnElse;

    [EventOccasion("In case we didn't have a file or its contents, or writing went wrong.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, LineVariableConstant).IsRereadRequired(out string? lineVariable))
            CurrentLineVariableName = lineVariable;

        string textToWrite;
        if (CurrentLineVariableName != null)
        {
            if (interaction.TryFindVariable(CurrentLineVariableName, out object? toWrite)
                && toWrite?.ToString() is string stringToWrite)
                textToWrite = stringToWrite;
            else
            {
                OnException?.Invoke(this, interaction.AppendRegister("nothing in variable"));
                return;
            }
        }
        else
        {
            var tsi = new TextSinkingInteraction(interaction);
            OnThen?.Invoke(this, tsi);
            textToWrite = tsi.ReadAllText();
        }

        var file = interaction.Register?.ToString();
        if (file == null)
        {
            OnException?.Invoke(this, interaction.AppendRegister("file expected in register"));
            return;
        }

        var info = new FileInfo(file);
        if (!Directory.Exists(info.Directory.FullName))
            Directory.CreateDirectory(info.Directory.FullName);
        try
        {
            using (var writer = new StreamWriter(info.FullName))
            {
                writer.Write(textToWrite);
            }
        }
        catch (Exception ex)
        {
            OnException?.Invoke(this, interaction.AppendRegister(ex));
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}