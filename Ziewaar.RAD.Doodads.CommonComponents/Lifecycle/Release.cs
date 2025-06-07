namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;

public class Release : IService
{
    [DefaultBranch]
    public event EventHandler<IInteraction> OnError;
    public event EventHandler<IInteraction> Name;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        string SourceSetting(EventHandler<IInteraction> forwardSourcing, string name, string fallback) =>
            (this, serviceConstants, interaction, forwardSourcing).SourceSetting(name, fallback);
        var name = SourceSetting(this.Name, "name", "default");
        if (interaction.TryGetClosest<ResidentialInteraction>(out var candidate, x => x.Name == name))
        {
            candidate.Leave();
        } else
        {
            OnError?.Invoke(this, VariablesInteraction.ForError(interaction, "Cannot terminate non-existent residence."));
        }
    }
}
