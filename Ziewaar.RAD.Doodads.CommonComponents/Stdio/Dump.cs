#nullable enable
using Newtonsoft.Json;

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
#pragma warning disable 67
[Category("Console")]
[Title("Dump the full context to console")]
[Description("Dont do this in prod.")]
public class Dump : IService
{
    [PrimarySetting("Name of the dump for finding it in the log.")]
    private readonly UpdatingPrimaryValue DumpNameConstant = new();
    [NamedSetting("limit", "Maximum depth to dump before stopping")]
    private readonly UpdatingKeyValue LimitConstant = new("limit");
    private string? CurrentDumpName;
    private int CurrentDumpLimit;
    [EventOccasion("After dump has happened. Logs before and after OnThen")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [NeverHappens] public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, DumpNameConstant).IsRereadRequired(out string? dumpName) && dumpName != null)
            this.CurrentDumpName = dumpName;
        if ((constants, LimitConstant).IsRereadRequired(out object? limit))
        {
            try
            {
                this.CurrentDumpLimit = Convert.ToInt32(limit);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "Dump limit formatted incorrectly"));
                this.CurrentDumpLimit = -1;
            }
        }
        var dumpHeader = "Untitled";
        if (CurrentDumpName != null)
            dumpHeader = CurrentDumpName;
        dumpHeader = $"{nameof(Dump)}|{dumpHeader}|{GlobalStopwatch.Instance.ElapsedMilliseconds:x8}";
        var depth = 0;
        GlobalLog.Instance?.Debug("[{dumpHeader}|Start]", dumpHeader);

        for (var working = interaction; working is not StopperInteraction; working = working.Stack)
        {
            var dumpBlob = JsonConvert.SerializeObject(new
            {
                Type = working.GetType().Name,
                Memory = working.Memory.Select<KeyValuePair<string, object>, (string, object)>(x => (x.Key, x.Value switch
                {
                    IEnumerable<string> strEnumerable => strEnumerable,
                    IEnumerable<object> objEnumerable => objEnumerable.Select(x => x.ToString()),
                    object anything => anything.ToString(),
                    _ => "Don't know"
                })).ToArray(),
                Register = working.Register.ToString()
            }, Formatting.Indented);
            GlobalLog.Instance?.Debug("[{dumpHeader}|{depth}] : {blob}", dumpHeader,
                $"{depth++:0000}", dumpBlob);
            if (CurrentDumpLimit != -1 && depth > CurrentDumpLimit)
                break;
        }
        GlobalLog.Instance?.Debug("[{dumpHeader}|OnThen Counting]", dumpHeader);
        var offset = GlobalStopwatch.Instance.ElapsedMilliseconds;
        OnThen?.Invoke(this, interaction);
        var duration = GlobalStopwatch.Instance.ElapsedMilliseconds - offset;
        GlobalLog.Instance?.Debug("[{dumpHeader}|OnThen Finished] {duration}ms", dumpHeader, duration);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}