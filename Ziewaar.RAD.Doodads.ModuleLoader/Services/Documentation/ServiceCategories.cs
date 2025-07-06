#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Documentation;
[Category("Reflection")]
[Title("Find service category names")]
[Description("""
             For the currently running doodads instance, explores all loaded assemblies for 
             IService implementations and finds the distinct categories these exist in.
             """)]
public class ServiceCategories : IService
{
    [EventOccasion("A list with 0 or more category names")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [NeverHappens] public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this,
        new CommonInteraction(interaction, DocumentationRepository.Instance.GetCategories()));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}