#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class CircularPop : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {

        string? loopName = null;

        var tsi = new TextSinkingInteraction(interaction);
        OnElse?.Invoke(this, tsi);
        loopName = tsi.ReadAllText();

        if (string.IsNullOrWhiteSpace(loopName) || loopName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "loop must have name"));
            return;
        }
        if (!CircularPush.Stacks.TryGetValue(loopName, out var circularStack))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "cant pop from non-existing stack"));
            return;
        }
        OnThen?.Invoke(this, new CommonInteraction(interaction, circularStack.Pop()));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
