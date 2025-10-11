#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.ScopeMemory;
#pragma warning disable 67
[Category("Memory & Register")]
[Title("Hard-code a value into register")]
[Description("""
             Put a value in the primary constant and it'll be in Register at OnThen
             """)]
public class Register : IService
{
    [EventOccasion("Primary constant comes out in register here")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When no register value was present")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (constants.PrimaryConstant is decimal newDecimal)
            OnThen?.Invoke(this, new CommonInteraction(interaction, newDecimal));
        else if (constants.PrimaryConstant.ToString() is string newValue)
            OnThen?.Invoke(this, new CommonInteraction(interaction, newValue));
        else
            OnException?.Invoke(this, new CommonInteraction(interaction, "no new register value"));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
