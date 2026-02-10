#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("At Most One At A Time")]
[Description("""
    Passes through control transparently, but makes sure of whatever is running 
    on the OnThen branch, it's only one; the rest of the interactions will be dropped.
    """)]
public class Latch : IService
{
    private readonly object lockObject = new();
    private bool isCaptured = false;
    [EventOccasion("Runs one at a time using the provided interaction")]
    public event CallForInteraction? OnThen;
    [EventOccasion("If someone else is in here already, this happens.")]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (isCaptured)
        {
            OnElse?.Invoke(this, interaction);
            return;
        }
        lock (lockObject)
        {
            isCaptured = true;
            OnThen?.Invoke(this, interaction);
            isCaptured = false;
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

[Category("Scheduling & Flow")]
[Title("At Most One At A Time")]
[Description("""
    Passes through control transparently, but makes sure of whatever is running 
    on the OnThen branch, it's only one; the rest of the interactions will be dropped.
    """)]
public class BlockingLatch : IService
{
    private readonly object lockObject = new();
    private bool isCaptured = false;
    [EventOccasion("Runs one at a time using the provided interaction")]
    public event CallForInteraction? OnThen;
    [EventOccasion("If someone else is in here already, this happens.")]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        lock (lockObject)
        {
            OnThen?.Invoke(this, interaction);     
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
