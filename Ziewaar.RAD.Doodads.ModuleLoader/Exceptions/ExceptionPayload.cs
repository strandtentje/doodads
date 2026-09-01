#nullable enable
using Ejije.Logging;
using Ziewaar.RAD.Doodads.ModuleLoader.RkopLanguage.Text;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Exceptions;

public class ExceptionPayload
{
    private static long FailCounter = 1;
    public ExceptionPayload(StampedMap? consts, Type? type, CursorText? text, IInteraction? interaction)
    {
        this.FailIndex = Interlocked.Increment(ref FailCounter);
        this.BackingInteraction = interaction;
        PrimaryConstant = consts?.PrimaryConstant?.ToString() ?? "No Primary Constant";
        PrimaryConstantStamp = consts?.PrimaryLog ?? -1;
        Constants = BuildConstants(consts);
        CurrentTimeStamp = GlobalStopwatch.Instance.ElapsedTicks;
        LastRegister = interaction?.Register?.ToString().Split('\n') ?? ["Empty Register"];
        Type = type?.Name ?? "Unknown Type";
        Directory = text?.WorkingDirectory?.FullName ?? "No Directory";
        File = text?.BareFile ?? "No File";
        Line = text?.GetCurrentLine() ?? -1;
        Column = text?.GetCurrentCol() ?? -1;
        Memory = (interaction?.Memory ?? EmptyReadOnlyDictionary.Instance).
            Select(x => (x.Key, x.Value.ToString())).
            ToDictionary(x => x.Key, x => x.Item2);
    }

    public string[] RegisterHistory
    {
        get
        {
            string? lastInteractionType = null;
            string? lastRegister = null;
            List<string> history = new();

            IInteraction? workingInteraction = BackingInteraction;

            while (workingInteraction != null && history.Count < 7)
            {
                if (workingInteraction.GetType().Name != (lastInteractionType ?? "") || 
                    (workingInteraction.Register.ToString() ?? "") != (lastRegister ?? ""))
                {
                    lastInteractionType = workingInteraction.GetType().Name;
                    lastRegister = workingInteraction.Register.ToString() ?? "Non-string register";
                    history.Add($"{lastInteractionType} => {lastRegister}");
                }

                workingInteraction =
                    workingInteraction.Stack is not StopperInteraction
                    ? workingInteraction.Stack : null;
            }

            if (workingInteraction != null)
                history.Add("Interaction => Register History Truncated");

            return history.ToArray();
        }
    }

    private IEnumerable<DiagnosticConstant> BuildConstants(StampedMap? consts)
    {
        if (consts == null) yield break;
        DiagnosticConstant? premature = null;
        DiagnosticConstant? result = null;
        string[] keys = [];
        try
        {
            keys = consts.NamedItems.Keys.Concat(consts.ChangeLog.Keys).Distinct().OrderBy(x => x).ToArray();
        }
        catch (Exception ex)
        {
            premature = new DiagnosticConstant("Exception while itemizing settings", 0, ex);
        }

        if (premature != null)
            yield return premature;

        foreach (var key in keys)
        {
            result = null;
            try
            {
                if (!consts.ChangeLog.TryGetValue(key, out var log))
                    log = -1;
                if (!consts.NamedItems.TryGetValue(key, out var val))
                    val = "NULL or EMPTY";
                result = new DiagnosticConstant(key, log, val);
            }
            catch (Exception ex)
            {
                result = new DiagnosticConstant($"Exception while itemizing setting {key}", 0, ex);
            }
            yield return result;
        }
    }


    private static readonly Log
        AnnouncementLog = Log.Fail(
            "Failure {index} - [{service}] indicates exceptional situation having",
            "{directory} {file} {line} {column} {primary} with",
            "{primarystamp} {timetstamp}"),
        RegisterHistoryLog = Log.Tech("Reg. History {index} - [{distance}] {value}"),
        MemoryValues = Log.Tech("Mem. Values {index} - [{key}] {value}"),
        DiagConst = Log.Tech("Diag. Values {index} - [{key}] {value} ({timestamp})");

    internal void PrintToLog()
    {
        Log.Post(AnnouncementLog, FailIndex, Type, Directory, File, Line, Column, PrimaryConstant, PrimaryConstantStamp, CurrentTimeStamp);
        for (int i = 0; i < LastRegister.Length; i++)
            Log.Post(RegisterHistoryLog, FailIndex, string.Format("{0}/{1}", i, LastRegister.Length), LastRegister[i]);
        foreach (var item in Memory)
            Log.Post(MemoryValues, FailIndex, item.Key, item.Value);
        foreach (var item in Constants)
            Log.Post(DiagConst, FailIndex, item.Key, item.Value, item.Timestamp);
    }

    public readonly string[] LastRegister;
    private long FailIndex;
    [JsonIgnore]
    private readonly IInteraction? BackingInteraction;
    public readonly string Type, Directory, File, PrimaryConstant;
    public readonly long PrimaryConstantStamp, CurrentTimeStamp;
    public readonly int Line, Column;
    public readonly IReadOnlyDictionary<string, string> Memory;
    public readonly IEnumerable<DiagnosticConstant> Constants;
}
