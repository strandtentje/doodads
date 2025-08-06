#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Deprecated")]
[Title("Add to infinite stack")]
[Description("""
             Access a stack globally available under a name.
             Adds the current register to it.
             """)]
public class CircularPush : IService
{
    public static readonly SortedList<string, CircularStack<object>> Stacks = new();        

    [NeverHappens]
    public event CallForInteraction? OnThen;
    [EventOccasion("Sink name of stack here")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When no name was provided.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        string? stackName = null;

        var tsi = new TextSinkingInteraction(interaction);
        OnElse?.Invoke(this, tsi);
        stackName = tsi.ReadAllText();

        if (string.IsNullOrWhiteSpace(stackName) || stackName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "loop must have name"));
            return;
        }
        if (!Stacks.TryGetValue(stackName, out var circularStack))
            circularStack = Stacks[stackName] = new();
        circularStack.Push(interaction.Register);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
