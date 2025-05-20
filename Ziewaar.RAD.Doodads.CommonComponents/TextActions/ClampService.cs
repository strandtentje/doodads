using System.Globalization;

namespace Define.Content.AutomationKioskShell.ValidationNodes;

public class ClampService : IService
{
    [NamedBranch]
    public event EventHandler<IInteraction> OnError;
    [NamedBranch]
    public event EventHandler<IInteraction> Clamped;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        var variableName = serviceConstants.InsertIgnore("var", "number");
        var inputVariable = serviceConstants.InsertIgnore("in", variableName);
        var outputVariable = serviceConstants.InsertIgnore("out", variableName);
        var defaultDecimal = serviceConstants.InsertIgnore("default", "0");

        decimal toClamp = decimal.TryParse(
            defaultDecimal, NumberStyles.Any, CultureInfo.InvariantCulture, out var candidateDefault) ?
            candidateDefault : 0M;

        var min = serviceConstants.InsertIgnore("min", 0M);
        var max = serviceConstants.InsertIgnore("max", 1M);
        var fmt = serviceConstants.InsertIgnore("format", "0000.00");

        if (interaction.TryFindVariable<decimal>(inputVariable, out var foundNumber))
        {
            toClamp = foundNumber;
        } else if (interaction.TryFindVariable<string>(inputVariable, out var foundNumberText) && 
            decimal.TryParse(foundNumberText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedNumberText))
        {
            toClamp = parsedNumberText;
        }
        var clamped = Math.Max(Math.Min(toClamp, max), min);
        var formatted = clamped.ToString(fmt);
        Clamped?.Invoke(this, new VariablesInteraction(interaction, new SortedList<string, object> { { outputVariable, formatted } }));
    }
}
