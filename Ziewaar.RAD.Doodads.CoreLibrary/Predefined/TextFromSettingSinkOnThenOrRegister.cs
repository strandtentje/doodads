#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public abstract class TextFromSettingSinkOnThenOrRegister : IService
{
    private string? TextFromSetting;
    [EventOccasion("Sink value that may also be expected in register, here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public virtual event CallForInteraction? OnElse;
    [NeverHappens]
    public virtual event CallForInteraction? OnException;

    protected abstract IUpdatingValue SettingToCheckFirst { get; }
    protected abstract string? Default { get; }
    protected abstract void Continue(StampedMap constants, IInteraction interaction, string text);
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, SettingToCheckFirst).IsRereadRequired(out string? candidateFromSetting))
            this.TextFromSetting = candidateFromSetting;

        string? textToUse = this.TextFromSetting;
        textToUse ??= SinkTextFromThen(interaction);
        textToUse ??= interaction.ToString();
        textToUse ??= Default;

        if (textToUse != null)
            Continue(constants, interaction, textToUse);
        else if (textToUse == null)
            OnException?.Invoke(this, interaction.AppendRegister("no text found"));
    }

    private string? SinkTextFromThen(IInteraction interaction)
    {
        var tsi = new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, tsi);
        var txt = tsi.ReadAllText();
        if (!string.IsNullOrWhiteSpace(txt))
            return txt;
        else
            return null;
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
