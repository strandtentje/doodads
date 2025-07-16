#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;

[Category("Text in register")]
[Title("Makes sure text in register is not too long")]
[Description("""
    Takes text in register and if its longer than the primary setting specifies,
    it'll be chopped on the right side.
    """)]
public class Truncate : IService
{
    private readonly UpdatingPrimaryValue LengthConstant = new();
    private decimal CurrentTruncLength = -1;

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, LengthConstant).IsRereadRequired(out string? candidateLengthText))
        {
            CurrentTruncLength = -1;
            if (candidateLengthText != null && decimal.TryParse(candidateLengthText,out var candidateLengthNumber))
            {
                this.CurrentTruncLength = candidateLengthNumber;
            }
        }
        if (CurrentTruncLength == -1)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "length required as primary param"));
            return;
        }
        var text = interaction.Register.ToString();
        if (text.Length > CurrentTruncLength)
            text = text.Substring(0, (int)CurrentTruncLength);
        OnThen?.Invoke(this, new CommonInteraction(interaction, text));            
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
