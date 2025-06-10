#pragma warning disable 67
#nullable enable
using System.Collections;
namespace Define.Content.AutomationKioskShell.ValidationNodes;
public class Pop : IService
{
    private readonly UpdatingPrimaryValue ListSourceNameConstant

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
                IEnumerable Continue()
                {
                    while (tor.MoveNext())
                        yield return tor.Current;
                }

                if (tor.MoveNext())
                {
                    OnThen?.Invoke(this, new CommonInteraction(interaction, tor.Current));
                    OnElse?.Invoke(this, new CommonInteraction(interaction, Continue()));
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