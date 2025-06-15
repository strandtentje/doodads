#pragma warning disable 67
#nullable enable
using System.Collections;
namespace Define.Content.AutomationKioskShell.ValidationNodes;
[Category("Lists and Items")]
[Title("Pop an item from an enumeration and continue")]
[Description("""
             For an enumeration in Register, or optionally in memory if a memory name is provided via 
             the primary setting, it will take the first item from the enumeration and move along.
             In case the list was taken from Register, enumeration cannot be continued after the first
             item. 
             If the list is taken from Memory as configured using the primary setting, it will advance 
             the list cursor and put that back in memory again in the same name. The item will go into
             register. 
             """)]
public class Pop : IService
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
        bool passOnEnumerable = false;
        
        if (listSourceName == null)
            list = interaction.Register as IEnumerable;
        else if (interaction.TryFindVariable<IEnumerable>(listSourceName, out var candidate))
        {
            list = candidate;
            passOnEnumerable = true;
        }
        
        if (list is IEnumerable enumerable)
        {
            var tor = enumerable.GetEnumerator();
            try
            {
                IEnumerable Continue()
                {
                    while (tor.MoveNext())
                        yield return tor.Current;
                }

                if (tor.MoveNext())
                {
                    if (passOnEnumerable)
                    {
                        OnThen?.Invoke(this, new CommonInteraction(interaction, tor.Current, new (1)
                        {
                            { listSourceName!, Continue() }
                        }));
                    }
                    else
                    {
                        OnThen?.Invoke(this, new CommonInteraction(interaction, tor.Current));
                    }
                }
                else
                {
                    OnElse?.Invoke(this, interaction);
                }
            }
            finally
            {
                if (tor is IDisposable disposable) disposable.Dispose();
            }
        }
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Cannot iterate over non-enumerable"));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}