#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;

[Category("Text in register")]
[Title("Remove spaces at both ends of text")]
[Description("""
    Removes spaces at ends of text in register, then continues.
    """)]
public class Trim : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this, new CommonInteraction(interaction, interaction.Register.ToString().Trim()));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
