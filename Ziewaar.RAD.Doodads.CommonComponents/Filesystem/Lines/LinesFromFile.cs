using Ziewaar.RAD.Doodads.CommonComponents.Filesystem.ReadWrite;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Lines;

[Category("System & IO")]
[Title("Read Lines from File")]
[Description("Iterate through lines in file, path from register.")]
public class LinesFromFile : IteratingService
{
    [EventOccasion("When the file was empty or didn't exist")]
    public override event CallForInteraction? OnElse;
    public event CallForInteraction? FileChanged;
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
        {
            List<string> prependLines = new List<string>();
            List<string> appendLines = new List<string>();
            var fli = new FileLineInteraction(repeater, ri.RepeatName);
            ri.IsRunning = true;
            while (ri.IsRunning)
            {
                ri.IsRunning = false;
                fli.LineNumber++;
                OnElse?.Invoke(this, repeater);
                if (fli.LineBefore is string lb)
                    prependLines.Insert(0, lb);
                if (fli.LineAfter is string la)
                    appendLines.Add(la);
                fli.LineBefore = fli.LineAfter = null;
            }
            if (prependLines.Any() || appendLines.Any())
            {
                File.WriteAllLines(candidateFilename, [.. prependLines]);
                File.WriteAllLines(candidateFilename, [.. appendLines]);
                FileChanged?.Invoke(this, repeater);
            }
        }
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
                        {
                            linesOutput.Add(fli.LineBefore);
                            fli.LineBefore = null;
                        }
                        if (!fli.SkipLine)
                        {
                            linesOutput.Add(fli.ChangedLine ?? linesInput[i]);
                            fli.ChangedLine = null;
                        } else
                        {
                            fli.SkipLine = false;
                        }
                        if (fli.LineAfter != null)
                        {
                            linesOutput.Add(fli.LineAfter);
                            fli.LineAfter = null;
                        }
                    }
                }
            }
            finally
            {
                if (changes)
                {
                    File.WriteAllLines(info.FullName, linesOutput.ToArray());
                    FileChanged?.Invoke(this, repeater);
                }
            }
        }
    }
}
