#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Documentation;
[Category("Reflection & Documentation")]
[Title("Find service category names")]
[Description("""
             For the currently running doodads instance, explores all loaded assemblies for 
             IService implementations and finds the distinct categories these exist in.
             """)]
public class ServiceCategories : IService
{
    [PrimarySetting("Name of Loop")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? RepeatName;
    [EventOccasion("Iterates for each service category while Continue is provided.")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [NeverHappens] public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? candidateRepeatName))
            this.RepeatName = candidateRepeatName;
        if (string.IsNullOrWhiteSpace(this.RepeatName) || this.RepeatName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }
        var categoryNames = DocumentationRepository.Instance.GetCategories();
        var ri = new RepeatInteraction(this.RepeatName, interaction);
        foreach (var categoryName in categoryNames)
        {
            if (!ri.IsRunning) break;
            ri.IsRunning = false;
            OnThen?.Invoke(this, new CommonInteraction(ri, categoryName));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
