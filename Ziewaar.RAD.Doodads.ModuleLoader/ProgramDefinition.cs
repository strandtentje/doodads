#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.ModuleLoader.Bridge;
using Ziewaar.RAD.Doodads.ModuleLoader.Exceptions;

namespace Ziewaar.RAD.Doodads.ModuleLoader;
#pragma warning disable CS0162
public class ProgramDefinition : IDisposable
{
    public ServiceDescription<ServiceBuilder> FirstServiceDescription =>
        CurrentSeries.Query<ServiceDescription<ServiceBuilder>>().FirstOrDefault() ??
        throw new StructureException("no service defined in file");
    public IService FirstServiceInstance =>
        (FirstServiceDescription.ResultSink?.CurrentService as DefinedServiceWrapper ??
         throw new StructureException("first service description wasn't a service")).Instance ??
        throw new StructureException("first service description had no instance");
    public string Name => (FirstServiceInstance is Definition)
        ? FirstServiceDescription.CurrentConstructor?.PrimarySettingValue as string ?? ""
        : throw new StructureException("first service must be definition");
    public bool IsPrimary => string.IsNullOrWhiteSpace(Name);
    public UnconditionalSerializableServiceSeries<ServiceBuilder> CurrentSeries { get; private set; } = new();
    public ServiceBuilder? CurrentBuilder => (CurrentSeries?.ResultSink as IInstanceWrapper) as ServiceBuilder;
    public IEntryPoint? EntryPoint => CurrentBuilder;
    public void Dispose() => CurrentBuilder?.Cleanup();
    public static bool TryCreate(ref CursorText cursor, out ProgramDefinition programDefinition)
    {
        programDefinition = new();
        try
        {
            if (programDefinition.CurrentSeries.UpdateFrom(Path.GetFileName(cursor.BareFile), ref cursor))
            {
                GlobalLog.Instance?.Information("found additional definition {name} in {file}", programDefinition.Name,
                    cursor.BareFile);
                return true;
            }
            else
            {
                GlobalLog.Instance?.Information("in file {file}, no more defs were found after {row}:{col}",
                    cursor.BareFile, cursor.GetCurrentLine(), cursor.GetCurrentCol());
                return false;
            }
        }
        catch (Exception ex)
        {
            GlobalLog.Instance?.Warning(ex,
                "stopped reading definitions in file {file} at {row}:{col} due to an exception; the syntax error is likely before.",
                cursor.BareFile, cursor.GetCurrentCol(), cursor.GetCurrentLine());
            return false;
        }
    }
}