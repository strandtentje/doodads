namespace Ziewaar.Web.Uploadserver.Pages;

public class DynamicPageInstructionEnumerable(string file) : IEnumerable<DynamicPageInstruction>
{
    private readonly object FileLock = new();
    private List<DynamicPageInstruction>? Instructions;
    public readonly string SourceFile = file;
    private DateTime LastChanged = DateTime.MinValue;

    public IEnumerator<DynamicPageInstruction> GetEnumerator()
    {
        lock (FileLock)
        {
            var currentDate = File.GetLastWriteTimeUtc(SourceFile);
            if (LastChanged != currentDate || Instructions == null)
            {
                LastChanged = currentDate;
                Rebuild();
            }
        }

        return Instructions?.GetEnumerator() ?? Enumerable.Empty<DynamicPageInstruction>().GetEnumerator();
    }

    private void Rebuild()
    {
        Instructions = new List<DynamicPageInstruction>();
        var fileText = new FileText(SourceFile);
        while(fileText.GoTo("{% ", out var offset, out var countUntil))
        {
            Instructions.Add(new DynamicPageInstruction(
                fileText, offset, countUntil, false));
            if (fileText.GoTo(" %}", out offset, out countUntil))
            {
                Instructions.Add(new DynamicPageInstruction(
                    fileText, offset, countUntil, true));
            }
        }
    }
        
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}