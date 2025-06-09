using System.Collections;

namespace Define.Content.AutomationKioskShell.ValidationNodes;

public class Option : IService
{
    private readonly UpdatingPrimaryValue MatchStringConst = new();
    public event EventHandler<IInteraction> OnThen;
    public event EventHandler<IInteraction> OnElse;
    public event EventHandler<IInteraction> OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, MatchStringConst).IsRereadRequired(out string matchString);

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
}