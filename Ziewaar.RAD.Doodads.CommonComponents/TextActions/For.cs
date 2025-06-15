#pragma warning disable 67
#nullable enable
using System.Collections;
namespace Define.Content.AutomationKioskShell.ValidationNodes;
[Category("Lists and Items")]
[Title("Loops over enumeration")]
[Description("""
             This loops over an enumeration in Register, and fires OnThen for each item, putting the Item
             in the Register. This is similar to Pop, except it'll keep on going on its own without needing to recurse.
             """)]
public class For : IService
{
    [EventOccasion("When an item was pulled from the enumeration")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the enumeration ends")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no enumeration was in Register")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
                OnThen?.Invoke(this, new CommonInteraction(interaction, item));
            OnElse?.Invoke(this, interaction);
        }
        else 
            OnException?.Invoke(this, new CommonInteraction(interaction, "Cannot iterate non-enumerable"));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}