#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
[Category("Reflection")]
[Title("Get the service at the root of a definition")]
[Description("""
             Provided with a definition name in register, and a path in memory, will produce special service information
             and put it in Register. This information can only be used by other reflection services.
             """)]
public class ServiceInDefinition : IService
{
    [EventOccasion("When the definition with name has been found, and has a service")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [EventOccasion(
        "Likely happens when path or name couldn't be determined, or no definition was found for the given name")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryFindVariable("path", out string? requestedPath) ||
            requestedPath == null ||
            interaction.Register is not string definitionName)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "path in memory and definition name in register required"));
            return;
        }
        var definition = ProgramRepository.Instance.GetForFile(requestedPath).Definitions
            ?.SingleOrDefault(x => x.Name == definitionName);
        if (definition == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "definition not found"));
            return;
        }
        OnThen?.Invoke(this, new CommonInteraction(interaction, definition.CurrentSeries));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}