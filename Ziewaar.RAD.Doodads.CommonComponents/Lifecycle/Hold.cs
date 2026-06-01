namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;

[Category("Scheduling & Flow")]
[Title("Blocks from here on out to prevent premature finishing of the execution")]
[Description("""
             Will pass on the interaction simply into OnThen, but will only return control when
             a Release was hit with the same name in the underlying block. This is useful for 
             keeping the application alive, or preventing requests from terminating prematurely,
             but should be using sparingly because it can cause application deadlocks.
             """)]
public class Hold : IService, IDisposable
{
    private readonly List<ResidentialInteraction> History = [];
    public void Dispose()
    {
        foreach (var item in History)
            item.Dispose();
    }
    [PrimarySetting("Name that the Release should also use.")]
    private readonly UpdatingPrimaryValue LockNameConstant = new();
    [EventOccasion("Happens before blocking")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Get blocking name dynamically")]
    public event CallForInteraction? GetName;
    [EventOccasion("Happens after underlying release was triggered")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when a name was not provided.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, LockNameConstant).IsRereadRequired(out string? lockName);
        var desiredName = lockName;
        if (desiredName == null || desiredName.Trim().Length == 0)
        {
            var tsi = new TextSinkingInteraction(interaction);
            GetName?.Invoke(this, tsi);
            desiredName = tsi.ReadAllText();
            if (desiredName.Trim().Length == 0)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "Hold Lock required name"));
                return;
            }
        }
        ResidentialInteraction currentResident;
        if (History.SingleOrDefault(x => x.Name == desiredName) is { } ri)
        {
            currentResident = ri;
        }
        else
        {
            currentResident = ResidentialInteraction.CreateBlocked(
                interaction, desiredName);
            History.Add(currentResident);
        }
        OnThen?.Invoke(this, currentResident);
        currentResident.Enter();
        History.Remove(currentResident);
        OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}