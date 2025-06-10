#nullable enable
using Ziewaar.RAD.Doodads.RKOP.Text;
namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class ExceptionPayload
{
    public ExceptionPayload(StampedMap? consts, Type? type, CursorText? text, IInteraction? interaction)
    {
        PrimaryConstant = consts?.PrimaryConstant?.ToString() ?? "No Primary Constant";
        PrimaryConstantStamp = consts?.PrimaryLog ?? -1;
        Constants = consts?.NamedItems.Join(consts?.ChangeLog, x => x.Key, x => x.Key, DiagnosticConstant.Create) ?? [];
        CurrentTimeStamp = GlobalStopwatch.Instance.ElapsedTicks;
        Register = interaction?.Register?.ToString() ?? "Empty Register";
        Type = type?.Name ?? "Unknown Type";
        Directory = text?.WorkingDirectory?.FullName ?? "No Directory";
        File = text?.BareFile ?? "No File";
        Line = text?.GetCurrentLine() ?? -1;
        Column = text?.GetCurrentCol() ?? -1;
        Memory = interaction?.Memory ?? EmptyReadOnlyDictionary.Instance;
    }
    public readonly string Register, Type, Directory, File, PrimaryConstant;
    public readonly long PrimaryConstantStamp, CurrentTimeStamp;
    public readonly int Line, Column;
    public readonly IReadOnlyDictionary<string, object> Memory;
    public readonly IEnumerable<DiagnosticConstant> Constants;
}
