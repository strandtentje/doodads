#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.ScopeMemory;

[Category("Memory & Register")]
[Title("Access Global Values")]
[Description("""
    Retrieve and update globals
    """)]
public class GlobalMemory : IService
{
    private static
        SortedList<string,
            SortedList<string, object>> GroupedGlobals = new();

    private readonly UpdatingPrimaryValue GlobalGroupConstant = new();
    private string CurrentGlobalGroupName = "";

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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
