#nullable enable
using Ziewaar.RAD.Doodads.CommonComponents.Control;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
[Category("Sink")]
[Title("Buffer Sink")]
[Description("""
             Use this to proxy whatever is currently sinking.
             This will hold that data until control leaves the BufferSink;
             then it's flushed immediately.

             You may find this useful to hold back HTTP body content while headers
             are still being set, for example, when Redirecting or doing stuff with
             cookies.
             
             Use Continue to Flush.
             """)]
public class SinkPlug : IService
{
    private readonly UpdatingPrimaryValue ContinueNameConstant = new();
    private string? ContinueName;
    [EventOccasion("New sink shows up here and buffers until control is returned to BufferSink")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [EventOccasion("Likely when no original sink could be found")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ContinueNameConstant).IsRereadRequired(out string? continueNameCandidate))
            this.ContinueName = continueNameCandidate;
        if (string.IsNullOrWhiteSpace(ContinueName) || ContinueName == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Continue name must be specified or sink will never flush"));
            return;
        }
        if (!interaction.TryGetClosest<ISinkingInteraction>(out var trueSink) || trueSink == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Sink is required for this"));
            return;
        }
        var bsi = new BufferSinkInteraction(interaction, trueSink);
        var ri = new RepeatInteraction(ContinueName, bsi)
        {
            IsRunning = false
        };
        try
        {
            OnThen?.Invoke(this, ri);
        }
        finally
        {
            if (ri.IsRunning) bsi.Flush();
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}