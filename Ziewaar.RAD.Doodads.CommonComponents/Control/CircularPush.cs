#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class CircularPush : IService
{
    public static readonly SortedList<string, CircularStack<object>> Stacks = new();        

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
        if (!Stacks.TryGetValue(loopName, out var circularStack))
            circularStack = Stacks[loopName] = new();
        circularStack.Push(interaction.Register);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
