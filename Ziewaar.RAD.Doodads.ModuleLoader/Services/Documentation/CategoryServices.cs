#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Documentation;
[Category("Reflection & Documentation")]
[Title("Find services that belong to a category")]
[Description("""
             Provided a category name through the primary setting or the Register, it will list all 
             services that belong to it.
             """)]
public class CategoryServices : IteratingService
{
    protected override bool RunElse { get; }
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (repeater.Register is not string categoryName)
            throw new Exception("Category name required in register");

        var categoryTypeNames = DocumentationRepository.Instance.GetCategoryTypes(categoryName);

        return categoryTypeNames.Select(repeater.AppendRegister);
    }
}