#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class Amoat : IService
{
    private readonly object lockObject = new();
    private bool isCaptured = false;

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (isCaptured) return;
        lock (lockObject)
        {
            isCaptured = true;
            OnThen?.Invoke(this, interaction);
            isCaptured = false;
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
