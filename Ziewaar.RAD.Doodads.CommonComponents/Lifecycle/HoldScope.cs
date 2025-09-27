#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;
#pragma warning disable 67
[Category("Scheduling & Flow")]
[Title("Blocks departure from HoldScope after OnThen finishes")]
[Description("""
             Will pass control to OnThen as usual, but after OnThen finishes,
             will block until release is called with the lock name as set in the
             primary constant.
             """)]
public class HoldScope : IService
{
    [PrimarySetting("Name of the lock to use with Release")]
    private readonly UpdatingPrimaryValue LockNameConstant = new();
    private string? CurrentLockName;
    [EventOccasion("Executes this before blocking")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no lock name was provided")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, LockNameConstant).IsRereadRequired(out string? lockNameCandidate))
            CurrentLockName = lockNameCandidate;
        if (CurrentLockName == null || string.IsNullOrWhiteSpace(CurrentLockName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "lock name required"));
            return;
        }
        using var currentResident = ResidentialInteraction.CreateBlocked(interaction, CurrentLockName);
        OnThen?.Invoke(this, currentResident);
        currentResident.Enter();
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}