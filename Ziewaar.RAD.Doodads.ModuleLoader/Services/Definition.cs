#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
public class Definition : IService
{    
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;
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
