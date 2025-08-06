#pragma warning disable 67
#nullable enable
using System.Collections;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;
[Category("Deprecated")]
[Title("Ensure the list contains one item, and take it.")]
[Description("""
             Provided a list in Register, it will take one item, and make sure it was the only one.
             If that was the case, OnThen will continue. For any different amount of items, OnElse will be 
             triggered.
             """)]
public class Single : IService
{
    [EventOccasion("When the list contained exactly one item")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the list didn't contain exactly one item")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely occurs when the register didn't contain a list at all")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is IEnumerable enumerable)
        {
            var tor = enumerable.GetEnumerator();
            try
            {
                if (tor.MoveNext())
                {
                    var item = tor.Current;
                    if (!tor.MoveNext())
                    {
                        OnThen?.Invoke(this, new CommonInteraction(interaction, item));
                        return;
                    }
                }
                OnElse?.Invoke(this, interaction);
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