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
public class ServiceCategories : IteratingService
{
    protected override bool RunElse { get; }
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater) =>
        DocumentationRepository.Instance.GetCategories().Select(repeater.AppendRegister);
}
