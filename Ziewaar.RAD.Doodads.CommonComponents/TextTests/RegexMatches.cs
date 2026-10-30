#pragma warning disable 67
using System.Text.RegularExpressions;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTests;

[Category("Input & Validation")]
[Title("Check if register text matches regex")]
[Description("""
    Sinks an expression at Expression, and then validates the text in register
    against it. OnThen if matches, otherwise OnElse.
    """)]
public class RegexMatches : IService
{
    [EventOccasion("Sink regex here")]
    public event CallForInteraction? Expression;
    [EventOccasion("For each match in register")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When no matches were found in register")]
    public event CallForInteraction? OnElse;
    [NeverHappens]
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


[Category("Input & Validation")]
[Title("Escape text for use in regex")]
[Description("""
    Put text in register, it comes out escaped.
    """)]
public class RegexEscape : BasicService
{
    public override event CallForInteraction? OnThen;
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForNullOrEmpty(Register(interaction), "some text req'd in register", out var text);
        OnThen?.Invoke(this, interaction.AppendRegister(Regex.Escape(text)));
    }
}