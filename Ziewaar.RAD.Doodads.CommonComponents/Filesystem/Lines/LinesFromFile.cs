#nullable enable
using Ziewaar;
using Ziewaar.RAD.Doodads.CommonComponents.ScopeMemory;
using Ziewaar.RAD.Doodads.CommonComponents.Transform;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Lines;

[Category("System & IO")]
[Title("Read Lines from File")]
[Description("Iterate through lines in file, path from register.")]
public class LinesFromFile : IService
{
    [PrimarySetting("Name to continue reading lines")]
    private readonly UpdatingPrimaryValue ContinueNameConstant = new();
    private string? CurrentRepeatName;

    [EventOccasion("Line of file comes out here.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When no file or lines were found.")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When no path in register")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ContinueNameConstant).IsRereadRequired(out string? candidateRepeater))
            CurrentRepeatName = candidateRepeater;
        if (string.IsNullOrWhiteSpace(CurrentRepeatName) || CurrentRepeatName == null)
        {
            OnException?.Invoke(this, interaction.AppendRegister("Repeat name required"));
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
            OnElse?.Invoke(this, interaction);
            return;
        }

        var linesInput = GetLinesFromFile(info.FullName).ToArray();

        if (linesInput.Length == 0 || (linesInput.Length == 1 && string.IsNullOrWhiteSpace(linesInput[0])))
        {
            OnElse?.Invoke(this, interaction);
            return;
        }

        List<string> linesOutput = new List<string>(linesInput.Length);
        var ri = new RepeatInteraction(CurrentRepeatName, interaction);
        var fli = new FileLineInteraction(ri, CurrentRepeatName);
        ri.IsRunning = true;
        bool changes = false;
        for (int i = 0; i < linesInput.Length && ri.IsRunning; i++)
        {
            ri.IsRunning = false;
            fli.Register = linesInput[i];
            fli.LineNumber = i + 1;
            OnThen?.Invoke(this, fli);
            if (ri.IsRunning)
            {
                changes |= fli.IsChanged;
                if (fli.LineBefore != null)
                    linesOutput.Add(fli.LineBefore);
                if (!fli.SkipLine)
                    linesOutput.Add(fli.ChangedLine ?? linesInput[i]);
                if (fli.LineAfter != null)
                    linesOutput.Add(fli.LineAfter);
            }
        }
        if (changes)
            File.WriteAllLines(info.FullName, linesOutput.ToArray());
    }
    IEnumerable<string> GetLinesFromFile(string fileName)
    {
        using var reader = new StreamReader(fileName);
        while (!reader.EndOfStream)
            yield return reader.ReadLine();
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
