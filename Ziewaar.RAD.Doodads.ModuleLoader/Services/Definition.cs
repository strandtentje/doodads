#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
public class Definition : IService
{    
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.TryGetClosest<CallingInteraction>(out var ci))
        {
            OnThen?.Invoke(this, new CommonInteraction(interaction, constants.NamedItems));
        } else if (interaction.TryGetClosest<ISelfStartingInteraction>(out var ss))
        {
            OnThen?.Invoke(this, new CommonInteraction(interaction, constants.NamedItems));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
