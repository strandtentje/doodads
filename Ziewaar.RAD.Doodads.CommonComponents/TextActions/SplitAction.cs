namespace Define.Content.AutomationKioskShell.ValidationNodes;

public class StringSplitService : IService
{
    [NamedBranch]
    public event EventHandler<IInteraction> OnError;
    [NamedBranch]
    public event EventHandler<IInteraction> Item;
    [NamedBranch]
    public event EventHandler<IInteraction> First;
    [NamedBranch]
    public event EventHandler<IInteraction> FirstOfMany;
    [NamedBranch]
    public event EventHandler<IInteraction> Last;
    [NamedBranch]
    public event EventHandler<IInteraction> LastOfMany;
    [NamedBranch]
    public event EventHandler<IInteraction> Single;
    [NamedBranch]
    public event EventHandler<IInteraction> None;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        var variableName = serviceConstants.InsertIgnore("var", "text");
        var inputVariable = serviceConstants.InsertIgnore("in", variableName);
        var outputVariable = serviceConstants.InsertIgnore("out", variableName);        
        var textToSplit = serviceConstants.InsertIgnore("default", "");

        var wildcardName = serviceConstants.InsertIgnore("wildcardname", "item");
        var wildcardValueFormat = serviceConstants.InsertIgnore("wildcardvalue", "{0}");

        var flags = serviceConstants.InsertIgnore("settings", "trim,noempty");
        var separator = serviceConstants.InsertIgnore("separator", ",");
        bool mustTrim = flags.Contains("trim");
        bool removeEmpty = flags.Contains("noempty");

        if (interaction.TryFindVariable(inputVariable, out string candidateVariable))
            textToSplit = candidateVariable;

        var entries = textToSplit.
            Split(separator, StringSplitOptions.None).
            Select(x => mustTrim ? x.Trim() : x).
            Where(x => !removeEmpty || x.Length > 0).
            ToArray();

        if (entries.Length == 0)
        {
            None?.Invoke(this, interaction);
            return;
        }
        if (entries.Length > 0)
        {
            var firstEntryInteraction = VariablesInteraction.
                CreateBuilder(interaction).
                Add(outputVariable, entries[0]).
                Build();

            First?.Invoke(this, firstEntryInteraction);
            if (entries.Length == 1)
                Single?.Invoke(this, firstEntryInteraction);
            if (entries.Length > 1)
                FirstOfMany?.Invoke(this, firstEntryInteraction);
        }
        for (int i = 0; i < entries.Length; i++)
        {
            Item?.Invoke(this, VariablesInteraction.
                CreateBuilder(interaction).
                Add(wildcardName, string.Format(wildcardValueFormat, i)).
                Add(outputVariable, entries[i]).
                Build());
        }
        if (entries.Length > 0)
        {
            var lastEntryInteraction = VariablesInteraction.CreateBuilder(interaction).Add(outputVariable, entries[entries.Length - 1]).Build();
            Last?.Invoke(this, lastEntryInteraction);
            if (entries.Length > 1)
                LastOfMany?.Invoke(this, lastEntryInteraction);
        }
    }
}
