#pragma warning disable 67
#nullable enable
using System.Collections;

namespace Define.Content.AutomationKioskShell.ValidationNodes;

public class VariableOption : IService
{
    private readonly UpdatingPrimaryValue VariableName = new();
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, VariableName).IsRereadRequired(out string? variableName);

        if (variableName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "variable name required for variable option"));
            return;
        }

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
                OnThen?.Invoke(this, new CommonInteraction(interaction, Remainder(), new() { { variableName, enumerator.Current } }));
            else
                OnElse?.Invoke(this, interaction);
        }
        else
            OnElse?.Invoke(this, interaction);
    }
}