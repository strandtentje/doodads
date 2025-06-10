#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
public class VoidService : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public delegate void CallForInteraction(object sender, IInteraction interaction);