using System.Collections;

namespace Define.Content.AutomationKioskShell.ValidationNodes;

public class None : IService
{
    public event EventHandler<IInteraction> OnThen;
    public event EventHandler<IInteraction> OnElse;
    public event EventHandler<IInteraction> OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is IEnumerable enumerable)
        {
            var tor = enumerable.GetEnumerator();
            try
            {
                if (!tor.MoveNext())
                    OnThen?.Invoke(this, interaction);
                else 
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
}