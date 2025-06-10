#pragma warning disable 67
#nullable enable
using System.Collections;

namespace Define.Content.AutomationKioskShell.ValidationNodes;

public class Option : IService
{
    private readonly UpdatingPrimaryValue MatchStringConst = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction?OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, MatchStringConst).IsRereadRequired(out string? matchString);

        if (interaction.Register is IEnumerable enumerable)
        {
            var enumerableStrings = enumerable.OfType<object>()
                .Select(x => x is string xString ? xString : Convert.ToString(x));
            var enumerator = enumerableStrings.GetEnumerator();

            IEnumerable<string> Remainder()
            {
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }

            if (enumerator.MoveNext())
            {
                if (enumerator.Current == matchString)
                    OnThen?.Invoke(this, new CommonInteraction(interaction, Remainder()));
                else
                    OnElse?.Invoke(this, interaction);
            }
            else
            {
                if (matchString == null)
                    OnThen?.Invoke(this, new CommonInteraction(interaction));
                else 
                    OnElse?.Invoke(this, new CommonInteraction(interaction));
            }

            return;
        }

        if (interaction.Register is not string directStringToMatch)
        {
            directStringToMatch = Convert.ToString(interaction.Register);
        }

        if (directStringToMatch == matchString)
            OnThen?.Invoke(this, interaction);
        else
            OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}