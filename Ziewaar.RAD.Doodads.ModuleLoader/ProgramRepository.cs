namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class ProgramRepository
{
    public static readonly ProgramRepository Instance = new();
    private readonly SortedList<string, KnownProgram> Programs = new();
    public KnownProgram GetForFile(string filePath)
    {
        if (!Programs.TryGetValue(filePath, out var known))
            known = Programs[filePath] = ProgramFactory.Instance.CreateFor(filePath);
        return known;
    }
    public IEntryPoint GetEntryPointForFile(string filePath) =>
        GetForFile(filePath).EntryPoint;
    public KnownProgram[] GetKnownPrograms() => Programs.Values.ToArray();
}
