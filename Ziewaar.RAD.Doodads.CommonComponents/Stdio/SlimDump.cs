#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;

[Category("Diagnostics & Debug")]
[Title("Dump the partial context to console")]
[Description("Dont do this in prod.")]
public class SlimDump : IService
{
    private readonly UpdatingPrimaryValue DumpFormatConstant = new();
    private string DumpFormat = "";
    private List<string> DumpVarNames = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!DumpSwitch.IsEnabled || interaction.TryGetClosest<DumpStopper>(out var _)) return;
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

        GlobalLog.Instance?.Debug(messageTemplate: DumpFormat, propertyValues: values);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
