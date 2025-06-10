#pragma warning disable 67
#nullable enable
using System.Collections;
namespace Define.Content.AutomationKioskShell.ValidationNodes;
public class Pop : IService
{
    private readonly UpdatingPrimaryValue ListSourceNameConstant = new();

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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