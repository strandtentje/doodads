#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Documentation;
[Category("Reflection & Documentation")]
[Title("Find services that belong to a category")]
[Description("""
             Provided a category name through the primary setting or the Register, it will list all 
             services that belong to it.
             """)]
public class CategoryServices : IService
{
    [PrimarySetting("Name of Loop")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? RepeatName;
    [EventOccasion("A list with 0 or more service names provided a category")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no category name could be acquired")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? candidateRepeatName))
            this.RepeatName = candidateRepeatName;
        if (string.IsNullOrWhiteSpace(this.RepeatName) || this.RepeatName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }
        if (interaction.Register is not string categoryName)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Category name required in register"));
            return;
        }
        var categoryTypeNames = DocumentationRepository.Instance.GetCategoryTypes(categoryName);
        var ri = new RepeatInteraction(this.RepeatName, interaction);
        foreach (var typeName in categoryTypeNames)
        {
            if (!ri.IsRunning) break;
            ri.IsRunning = false;
            OnThen?.Invoke(this, new CommonInteraction(ri, typeName));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}