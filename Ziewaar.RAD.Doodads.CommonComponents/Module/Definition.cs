namespace Ziewaar.RAD.Doodads.CommonComponents.Module;
public class Definition : IService
{
    public static readonly SortedList<string, Definition> NamedDefinitions = new();
    private string _lastKnownName;
    [NamedBranch]
    public event EventHandler<IInteraction> OnError;
    [NamedBranch]
    public event EventHandler<IInteraction> Begin;
    public int CompareTo(Definition other)
    {
        throw new NotImplementedException();
    }
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        if (interaction.TryGetClosest<CallingInteraction>(out var ci))
        {
            Begin?.Invoke(this, new VariablesInteraction(interaction, serviceConstants));
        }
        else
        {
            var requestedName = serviceConstants.InsertIgnore("name", $"NewModule{Guid.NewGuid()}");
            if (!NamedDefinitions.ContainsKey(requestedName))
            {
                if (_lastKnownName is string lastSetName) NamedDefinitions.Remove(lastSetName);
                _lastKnownName = requestedName;
                NamedDefinitions.Add(_lastKnownName, this);
            }
        }
    }
}
