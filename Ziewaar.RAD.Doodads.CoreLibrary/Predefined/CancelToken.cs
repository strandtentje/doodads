#nullable enable

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

[Category("Scheduling & Flow")]
[Title("Invoke a cancellation token")]
[Description("Takes a scoped cancellation token and invokes it. Note, contrary to .net cancellation tokens, this one is reusable.")]
public class CancelToken : IService
{
    [PrimarySetting("Specify name of iterators and such to cancel. Leave blank for catch-all")]
    private readonly UpdatingPrimaryValue CancellationNameConstant = new();
    private string? CurrentCancellationToken;

    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, CancellationNameConstant).IsRereadRequired(out CurrentCancellationToken);

        var cancels = interaction.GetAllOf<CancellationInteraction>(
            x => x.Name == "" || x.Name == CurrentCancellationToken);

        foreach (var item in cancels)
            item.Cancel();
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}