#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
public class DefinitionSeries : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is not ProgramDefinition definition)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Expected definition from ListDefinitions"));
            return;
        }

        OnThen?.Invoke(this, new CommonInteraction(interaction, definition.CurrentSeries));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}