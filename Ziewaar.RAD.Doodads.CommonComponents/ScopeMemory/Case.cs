#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.ScopeMemory;

[Category("Memory & Register")]
[Title("Conditional branch on register value")]
[Shorthand("~PRIMARY")]
[Description("""
             Checks if the register's string-converted value is an exact match with the provided primary
             setting text. 
             """)]
public class Case : IService
{
    [PrimarySetting("String value to match the Register against")]
    private readonly UpdatingPrimaryValue MatchStringConst = new();
    [EventOccasion("When the string representation of the register matches the setting")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When there was no match")]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, MatchStringConst).IsRereadRequired(out string? matchString);

        var directStringToMatch =
            interaction.Register is string value ? value : Convert.ToString(interaction.Register);

        if (directStringToMatch == matchString)
            OnThen?.Invoke(this, interaction);
        else
            OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class DebounceContainer
{
    public object? LastValue;
}

public class DebounceIn : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this, interaction.AppendCustom(new DebounceContainer()));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class DebounceOut : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.TryGetCustom<DebounceContainer>(out var debouncer) && debouncer != null)
        {
            if (debouncer.LastValue?.ToString() != interaction.Register.ToString())
            {
                debouncer.LastValue = interaction.Register;
                OnThen?.Invoke(this, interaction);
            } else
            {
                OnElse?.Invoke(this, interaction);
            }
        } else
        {
            OnException?.Invoke(this, interaction.AppendRegister("debouncein required"));
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}