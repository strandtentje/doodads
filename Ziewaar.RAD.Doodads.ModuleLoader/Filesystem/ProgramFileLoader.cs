#nullable enable
using Ziewaar.RAD.Doodads.ModuleLoader.Exceptions;

#pragma warning disable CS0162 // Unreachable code detected
namespace Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;
public class ProgramFileLoader : IDisposable
{
    public IInteraction? AutoStartOnReloadParams;
    public List<ProgramDefinition>? Definitions;
    public readonly ResilientCursorTextEmitter Emitter;
    public ProgramFileLoader(ResilientCursorTextEmitter emitter)
    {
        this.Emitter = emitter;
        emitter.CursorTextAvailable += Emitter_HandleNewCursorText;
    }
    private void CleanDefinitions()
    {
        Definitions ??= new();
        foreach (var definition in Definitions)
            definition.Dispose();
        Definitions.Clear();
    }
    private void Emitter_HandleNewCursorText(object sender, CursorText text)
    {
        CleanDefinitions();
        var seenTerminator = true;
        while (seenTerminator && ProgramDefinition.TryCreate(ref text, out ProgramDefinition newDefinition))
        {
            Definitions!.Add(newDefinition);
            seenTerminator = false;
            Token terminator;
            do
            {
                text = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.Terminator, out terminator);
                seenTerminator |= terminator.IsValid;
            } while (terminator.IsValid);
        }
       
        text = text.SkipWhile(char.IsWhiteSpace);
        if (text.Position != text.Text.Length)
            throw new SyntaxException(text, "parsing stopped unexpectedly");
    }
    public void Dispose() => CleanDefinitions();
    public void Reload()
    {
        Emitter.RequestLoad();
        Emitter.WorkingState.SloppilyWaitForWorkToCease();
        if (AutoStartOnReloadParams != null)
            GetPrimaryEntryPoint().Run(this, AutoStartOnReloadParams);
    }
    public void Save(int attempts = 0) => Emitter.RequestSave(Definitions ?? []);
    public IEntryPoint GetPrimaryEntryPoint()
    {
        if (TryFindEntryPoint(x => string.IsNullOrWhiteSpace(x.Name), out var entryPoint) is not int resultCount)
            throw new StructureException($"primary entry point was accessed before the file was fully ingested");
        if (resultCount != 1)
            throw new StructureException(
                $"primary entry point was defined {resultCount} times, while it MUST be defined exactly once.");
        return entryPoint!;
    }
    public int? TryFindEntryPoint(Func<ProgramDefinition, bool> predicate, out IEntryPoint? entryPoint)
    {
        var countedEntryPoints = Definitions?.Where(predicate).Count();
        entryPoint = null;
        if (countedEntryPoints != 1)
            return countedEntryPoints;
        entryPoint = Definitions?.SingleOrDefault(predicate)?.EntryPoint ??
                     throw new StructureException("entry point declaration found but instance not assigned.");
        return 1;
    }
}