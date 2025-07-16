#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;

public class NewGuid : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this, new CommonInteraction(interaction, Guid.NewGuid().ToString()));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
