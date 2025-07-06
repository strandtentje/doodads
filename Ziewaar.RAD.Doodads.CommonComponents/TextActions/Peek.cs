#pragma warning disable 67
#nullable enable
using System.Collections;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;

[Category("Lists and Items")]
[Title("Peek an item from an enumeration and continue")]
[Description("""
             For an enumeration in Register, or optionally in memory if a memory name is provided via 
             the primary setting, it will look at the first item from the enumeration without taking it out.
             So it's like Pop, but non-destructive.
             """)]
public class Peek : IService
{
    [PrimarySetting("Optionally provide a memory name for getting the list from and putting it back into.")]
    private readonly UpdatingPrimaryValue ListSourceNameConstant = new();
    [EventOccasion("When an item was popped and the list was advanced; contains the item in register")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the list was empty")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when the memory location or register didn't contain a list.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, ListSourceNameConstant).IsRereadRequired(out string? listSourceName);

        IEnumerable? list = null;

        if (listSourceName != null && interaction.TryFindVariable<IEnumerable>(listSourceName, out var candidate))
        {
            list = candidate;
        }
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "must take item from memory, not register"));
            return;
        }

        if (list is IEnumerable enumerable)
        {
            if (enumerable.OfType<object>().Any())
            {
                OnThen?.Invoke(this, new CommonInteraction(interaction, enumerable.OfType<object>().First()));
            }
            else
            {
                OnElse?.Invoke(this, interaction);
            }
        }
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Cannot iterate over non-enumerable"));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}