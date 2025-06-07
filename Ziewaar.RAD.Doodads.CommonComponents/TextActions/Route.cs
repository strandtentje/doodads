namespace Define.Content.AutomationKioskShell.ValidationNodes;

public class Route : IService
{
    public event EventHandler<IInteraction> OnError;
    public event EventHandler<IInteraction> RouteVariable;
    public event EventHandler<IInteraction> Separator;
    public event EventHandler<IInteraction> SegmentVariable;
    [DefaultBranch]
    public event EventHandler<IInteraction> Segment;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        string SourceSetting(EventHandler<IInteraction> forwardSourcing, string name, string fallback) =>
            (this, serviceConstants, interaction, forwardSourcing).SourceSetting(name, fallback);
        var sepchar = SourceSetting(Separator, "separator", "/");
        var routevar = SourceSetting(RouteVariable, "route", "url");
        var segmentvar = SourceSetting(SegmentVariable, "segment", "variable");

        if (!interaction.TryFindVariable<Queue<string>>(routevar, out var queueCandidate))
        {
            if (interaction.TryFindVariable<string>(routevar, out var unsplitQueue))
                queueCandidate = new Queue<string>(unsplitQueue.Split(sepchar, StringSplitOptions.RemoveEmptyEntries));
            else
            {
                OnError?.Invoke(this, VariablesInteraction.ForError(interaction, "no route variable found"));
                return;
            }
        }

        if (!queueCandidate.Any())
        {
            OnError?.Invoke(this, VariablesInteraction.ForError(interaction, "out of route segments"));
            return;
        }

        Segment?.Invoke(this, VariablesInteraction.
            CreateBuilder(interaction).
            Add(segmentvar, queueCandidate.Dequeue()).
            Add(routevar, queueCandidate).
            Build());
    }
}
