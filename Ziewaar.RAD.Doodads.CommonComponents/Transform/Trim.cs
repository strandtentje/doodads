#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

[Category("Input & Validation")]
[Title("Remove spaces at both ends of text")]
[Description("""
    Removes spaces at ends of text in register, then continues.
    """)]
public class Trim : IService
{
    private readonly UpdatingPrimaryValue TrimCharactersConstant = new();
    private Func<char, bool> CurrentTrimCondition = char.IsWhiteSpace;
    [EventOccasion("Register string trimmed of spaces on both ends")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, TrimCharactersConstant).IsRereadRequired(out string? trimCharactersCandidate))
        {
            if (trimCharactersCandidate is string trimCharacters)
                CurrentTrimCondition = c => trimCharacters.Contains(c);
            else
                CurrentTrimCondition = char.IsWhiteSpace;
        }
        OnThen?.Invoke(this, new CommonInteraction(
            interaction, 
            new string([.. interaction.Register.ToString().
            SkipWhile(CurrentTrimCondition).
            Reverse().
            SkipWhile(CurrentTrimCondition).
            Reverse()])));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
