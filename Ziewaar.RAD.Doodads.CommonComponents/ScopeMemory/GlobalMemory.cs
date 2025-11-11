#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.ScopeMemory;

[Category("Memory & Register")]
[Title("Access Global Values")]
[Description("""
    Retrieve and update globals. All named settings will override the current
    globals.
    """)]
public class GlobalMemory : IService
{
    private static
        SortedList<string,
            SortedList<string, object>> GroupedGlobals = new();
    [PrimarySetting("Optionally specify a group name for the globals")]
    private readonly UpdatingPrimaryValue GlobalGroupConstant = new();
    private string CurrentGlobalGroupName = "";
    [EventOccasion("Globals are available in memory, here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, GlobalGroupConstant).IsRereadRequired(() => "", out string? nameCandidate))
            CurrentGlobalGroupName = nameCandidate ?? "";
        if (!GroupedGlobals.TryGetValue(CurrentGlobalGroupName, out var group))
            GroupedGlobals[CurrentGlobalGroupName] = group = new SortedList<string, object>();
        constants.CopyNamedTo(group);
        OnThen?.Invoke(this, interaction.AppendMemory(group));
    }
    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}
