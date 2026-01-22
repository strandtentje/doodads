#nullable enable
using Ziewaar.RAD.Doodads.ModuleLoader.Bridge;
using Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;

namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class ProgramRepository
{
    public static readonly ProgramRepository Instance = new();
    private readonly SortedList<string, ProgramFileLoader> Programs = new();
    public ProgramFileLoader GetForFile(string filePath, IInteraction? autoStartOnReloadParams = null)
    {
        if (!File.Exists(filePath) && File.Exists($"{filePath}.rkop"))
            filePath += ".rkop";
        if (!Programs.TryGetValue(filePath, out var known))
            known = Programs[filePath] = ProgramFactory.Instance.CreateFor(filePath, autoStartOnReloadParams);
        else 
            known.Reload();

        return known;
    }
    public void DisposeFile(string filePath)
    {
        if (!File.Exists(filePath) && File.Exists($"{filePath}.rkop"))
            filePath += ".rkop";
        if (Programs.TryGetValue(filePath, out var known))
        {
            Programs.Remove(filePath);
            known.Dispose();            
        }
    }
    public IEntryPoint? GetEntryPointForFile(string filePath) =>
        GetForFile(filePath).GetPrimaryEntryPoint();
    public ProgramFileLoader[] GetKnownPrograms() => Programs.Values.ToArray();
    public (string workingDirectory, string fileName) FindFileOf(IService service)
    {
        foreach (var programFileLoader in Programs)
        {
            foreach (var programDefinition in programFileLoader.Value.Definitions ?? [])
            {
                var potentialResult = programDefinition.CurrentSeries.Query<ServiceDescription<ServiceBuilder>>(x =>
                    x.ResultSink is ServiceBuilder sb &&
                    sb.CurrentService is DefinedServiceWrapper dsw &&
                    dsw.Instance == service).SingleOrDefault();

                var position = potentialResult?.TextScope;
                if (position != null)
                    return (position.WorkingDirectory.FullName, position.BareFile);
            }
        }
        throw new ArgumentOutOfRangeException($"service {service.GetType().Name} was provided but never found");
    }
}