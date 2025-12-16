#nullable enable
using System.Threading.Channels;
using Ziewaar;
using Ziewaar.RAD.Doodads.CommonComponents.ScopeMemory;
using Ziewaar.RAD.Doodads.CommonComponents.Transform;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Lines;

[Category("System & IO")]
[Title("Read Lines from File")]
[Description("Iterate through lines in file, path from register.")]
public class LinesFromFile : IteratingService
{
    [EventOccasion("When the file was empty or didn't exist")]
    public override event CallForInteraction? OnElse;
    protected override bool RunElse => false;
    IEnumerable<string> GetLinesFromFile(string fileName)
    {
        using var reader = new StreamReader(fileName);
        while (!reader.EndOfStream)
            yield return reader.ReadLine();
    }
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        RepeatInteraction ri = (RepeatInteraction)repeater;
        if (repeater.Register.ToString() is not string candidateFilename)
            throw new Exception("Path expected in register");
        else if (new FileInfo(candidateFilename) is not { } info
            || !info.Exists
            || GetLinesFromFile(info.FullName).ToArray() is not { } linesInput
            || linesInput.Length == 0
            || string.IsNullOrWhiteSpace(linesInput[0]))
            OnElse?.Invoke(this, repeater);
        else
        {
            var fli = new FileLineInteraction(repeater, ri.RepeatName);
            List<string> linesOutput = new List<string>(linesInput.Length);
            bool changes = false;
            try
            {
                for (int i = 0; i < linesInput.Length && ri.IsRunning; i++)
                {
                    ri.IsRunning = false;
                    fli.Register = linesInput[i];
                    fli.LineNumber = i + 1;
                    yield return fli;
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
            }
            finally
            {
                if (changes)
                    File.WriteAllLines(info.FullName, linesOutput.ToArray());
            }
        }
    }
}
