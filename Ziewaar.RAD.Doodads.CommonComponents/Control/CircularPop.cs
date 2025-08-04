#nullable enable
using System.Threading;

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
        CircularStack<object> circularStack;
        while (!CircularPush.Stacks.TryGetValue(loopName, out circularStack))
            Thread.Sleep(100);
        while (circularStack.Count == 0)
            Thread.Sleep(100);
        OnThen?.Invoke(this, new CommonInteraction(interaction, circularStack.Pop()));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
