#pragma warning disable 67
#nullable enable
using System.Collections;
namespace Define.Content.AutomationKioskShell.ValidationNodes;
public class Iterate : IService
{
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is IEnumerable enumerable)
            foreach (var item in enumerable)
                OnThen?.Invoke(this, new CommonInteraction(interaction, item));
        else 
            OnException?.Invoke(this, new CommonInteraction(interaction, "Cannot iterate non-enumerable"));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}