#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class BlockEvent : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ScopedEventInteraction>(out var ewh) || ewh == null)
            throw new Exception("No event was scoped.");
        ewh.Ewh.Reset();
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
