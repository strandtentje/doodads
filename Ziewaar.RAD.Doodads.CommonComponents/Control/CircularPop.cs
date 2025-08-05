#nullable enable
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Control")]
[Title("Consume from an infinite stack")]
[Description("""
             Take an item from an infinite stack;
             when the last item was reached, it wraps back around.
             """)]
public class CircularPop : IService
{
    [EventOccasion("Has stack item in register here")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Sink name of stack here.")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When no name was provided")]
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
        CircularStack<object> circularStack;
        while (!CircularPush.Stacks.TryGetValue(loopName, out circularStack))
            Thread.Sleep(100);
        while (circularStack.Count == 0)
            Thread.Sleep(100);
        OnThen?.Invoke(this, new CommonInteraction(interaction, circularStack.Pop()));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
