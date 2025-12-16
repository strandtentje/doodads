#nullable enable

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

[Category("Scheduling & Flow")]
[Title("Name a cancellation token")]
[Description("""Scopes a cancellation token in order to be able to cancel running jobs""")]
public class OpenCancellationToken : IService
{
    [PrimarySetting("Specify name of iterators and such to cancel. Leave blank for catch-all")]
    private readonly UpdatingPrimaryValue CancellationNameConstant = new();
    private string? CurrentCancellationToken;

    [EventOccasion("Cancellation token comes out here")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, CancellationNameConstant).IsRereadRequired(out CurrentCancellationToken);
        OnThen?.Invoke(this, new CancellationInteraction(interaction, CurrentCancellationToken ?? ""));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
