#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Reflection;
[Category("Reflection")]
[Title("Find services that belong to a category")]
[Description("""
             Provided a category name through the primary setting or the Register, it will list all 
             services that belong to it.
             """)]
public class CategoryServices : IService
{
    [PrimarySetting("Optionally hardcoded category name")]
    private readonly UpdatingPrimaryValue CategoryName = new();
    [EventOccasion("A list with 0 or more service names provided a category")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no category name could be acquired")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, CategoryName).IsRereadRequired<string>(out var hardcodedCategoryName);
        var categoryName = hardcodedCategoryName ?? interaction.Register as string;
        if (categoryName == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "category name required via register or primary setting"));
            return;
        }
        OnThen?.Invoke(this,
            new CommonInteraction(interaction, DocumentationRepository.Instance.GetCategoryTypes(categoryName)));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}