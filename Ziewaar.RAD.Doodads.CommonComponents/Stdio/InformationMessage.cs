#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;

[Category("Diagnostics & Debug")]
[Title("Write an information message and optionally some interaction context to console/logfile")]
[Description("This is cool in prod.")]
[ShortNames("msg")]
public class InformationMessage : IService
{
    [PrimarySetting("Serilog-style log line that may optionally contain memory names")]
    private readonly UpdatingPrimaryValue DumpFormatConstant = new();
    private string DumpFormat = "";
    private List<string> DumpVarNames = [];
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, DumpFormatConstant).IsRereadRequired(out string? formatCandidate))
        {
            DumpFormat = formatCandidate ?? "";
            DumpVarNames = formatCandidate?.
                Split(['{'], StringSplitOptions.RemoveEmptyEntries).
                Select(tagCandidate => (startOfTag: tagCandidate, posOfEnd: tagCandidate.IndexOf('}'))).
                Where(x => x.posOfEnd > -1).
                Select(x => x.startOfTag.Substring(0, x.posOfEnd)).
                ToList() ?? [];

            foreach (var item in DumpVarNames)
                DumpFormat = DumpFormat.Replace(item, item.Replace(" ", ""));
        }

        object[] values = DumpVarNames.
            Select(varName => interaction.TryFindVariable(varName, out object? cv) ? cv?.ToString() ?? "" : "").
            OfType<object>().
            ToArray();

        GlobalLog.Instance?.Information(messageTemplate: DumpFormat, propertyValues: values);

        OnThen?.Invoke(this, interaction);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
