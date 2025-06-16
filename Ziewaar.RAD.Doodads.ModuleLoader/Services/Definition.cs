#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
[Category("Call Definition Return")]
public class Definition : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction is CallingInteraction ci)
        {
            OnThen?.Invoke(this, new CommonInteraction(interaction, memory: constants.ToSortedList()));
        }
        else if (interaction is ISelfStartingInteraction ss)
        {
            OnThen?.Invoke(this, new CommonInteraction(interaction, memory: constants.ToSortedList()));
        }
        else
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    register: "Warning: should not be used in the middle of a program."));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}