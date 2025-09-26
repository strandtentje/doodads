#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.ScopeMemory;
#pragma warning disable 67
public class Register : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (constants.PrimaryConstant.ToString() is string newValue)
            OnThen?.Invoke(this, new CommonInteraction(interaction, newValue));
        else
            OnException?.Invoke(this, new CommonInteraction(interaction, "no new register value"));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}