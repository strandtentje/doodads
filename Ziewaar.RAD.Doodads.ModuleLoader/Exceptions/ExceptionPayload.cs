#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Exceptions;

public class ExceptionPayload
{
    public ExceptionPayload(StampedMap? consts, Type? type, CursorText? text, IInteraction? interaction)
    {
        PrimaryConstant = consts?.PrimaryConstant?.ToString() ?? "No Primary Constant";
        PrimaryConstantStamp = consts?.PrimaryLog ?? -1;
        Constants = consts?.NamedItems.Join(consts?.ChangeLog, x => x.Key, x => x.Key, DiagnosticConstant.Create) ?? [];
        CurrentTimeStamp = GlobalStopwatch.Instance.ElapsedTicks;
        Register = interaction?.Register?.ToString().Split('\n') ?? ["Empty Register"];
        Type = type?.Name ?? "Unknown Type";
        Directory = text?.WorkingDirectory?.FullName ?? "No Directory";
        File = text?.BareFile ?? "No File";
        Line = text?.GetCurrentLine() ?? -1;
        Column = text?.GetCurrentCol() ?? -1;
        Memory = (interaction?.Memory ?? EmptyReadOnlyDictionary.Instance).
            Select(x => (x.Key, x.Value.ToString())).
            ToDictionary(x => x.Key, x => x.Item2);
    }
    public readonly string[] Register;
    public readonly string  Type, Directory, File, PrimaryConstant;
    public readonly long PrimaryConstantStamp, CurrentTimeStamp;
    public readonly int Line, Column;
    public readonly IReadOnlyDictionary<string, string> Memory;
    public readonly IEnumerable<DiagnosticConstant> Constants;
}
