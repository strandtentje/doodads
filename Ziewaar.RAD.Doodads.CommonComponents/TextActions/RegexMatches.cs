#pragma warning disable 67
#nullable enable
using System.Text.RegularExpressions;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;

[Category("Text in register")]
[Title("Check if register text matches regex")]
[Description("""
    Sinks an expression at Expression, and then validates the text in register
    against it. OnThen if matches, otherwise OnElse.
    """)]
public class RegexMatches : IService
{
    public event CallForInteraction? Expression;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var tsi = new TextSinkingInteraction(interaction);
        Expression?.Invoke(this, tsi);
        Regex currentExpression;
        string testText;
        try
        {
            currentExpression = new Regex(tsi.ReadAllText());
            testText = interaction.Register.ToString();
        }
        catch (Exception ex)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, ex));
            return;
        }
        var matches = currentExpression.Matches(testText)?.OfType<Match>().ToArray() ?? [];
        if (matches.Length == 0)
        {
            OnElse?.Invoke(this, interaction);
            return;
        }

        foreach (var item in matches)
        {
            var groupDict = currentExpression.GetGroupNames().ToDictionary(x => x, x => (object)item.Groups[x].Value);
            OnThen?.Invoke(this, new CommonInteraction(interaction, memory: groupDict, register: item.Value));
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
