#pragma warning disable 67
#nullable enable
using System.Collections;
namespace Define.Content.AutomationKioskShell.ValidationNodes;
public class Single : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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