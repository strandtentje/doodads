#pragma warning disable 67
#nullable enable
using System.Collections;

namespace Define.Content.AutomationKioskShell.ValidationNodes;
public class Case : IService
{
    private readonly UpdatingPrimaryValue MatchStringConst = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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