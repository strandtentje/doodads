#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Control")]
[Title("At Most One At A Time")]
[Description("""
    Passes through control transparently, but makes sure of whatever is running 
    on the OnThen branch, it's only one; the rest of the interactions will be dropped.
    """)]
public class Amoat : IService
{
    private readonly object lockObject = new();
    private bool isCaptured = false;
    [EventOccasion("Runs one at a time using the provided interaction")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
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
