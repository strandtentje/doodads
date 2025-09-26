#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;
public class HoldScope : IService
{
    private readonly UpdatingPrimaryValue LockNameConstant = new();
    private string? CurrentLockName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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