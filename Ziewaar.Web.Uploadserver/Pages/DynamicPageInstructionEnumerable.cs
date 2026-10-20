namespace Ziewaar.Web.Uploadserver.Pages;

public class DynamicPageInstructionEnumerable : IEnumerable<DynamicPageInstruction>
{
    private readonly object FileLock = new object();
    private List<DynamicPageInstruction> Instructions = null;
    public readonly string SourceFile;
    private DateTime LastChanged = DateTime.MinValue;
    public DynamicPageInstructionEnumerable(string file)
    {
        this.SourceFile = file;
    }

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

        return Instructions.GetEnumerator();
    }

    private void Rebuild()
    {
        Instructions = new List<DynamicPageInstruction>();
        var fileText = new FileText(SourceFile);
        int offset, countUntil;
        while(fileText.GoTo("{% ", out offset, out countUntil))
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