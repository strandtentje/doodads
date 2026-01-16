#nullable enable
#pragma warning disable 67
using Ziewaar.RAD.Doodads.CommonComponents.Control;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
[Category("Sourcing & Sinking")]
[Title("Buffer Sink")]
[Description("""
             Use this to proxy whatever is currently sinking.
             This will hold that data until control leaves the BufferSink;
             then it's flushed immediately.

             You may find this useful to hold back HTTP body content while headers
             are still being set, for example, when Redirecting or doing stuff with
             cookies.
             
             Use Continue to Flush; if no Continue is used, accumulated data will be rejected.
             """)]
public class SinkPlug : IService
{
    [PrimarySetting("Continue-name for this service")]
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
        
        if (!interaction.TryGetClosest<ISinkingInteraction>(out var trueSink) || trueSink == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Sink is required for this"));
            return;
        }
        var bsi = new BufferSinkInteraction(interaction, trueSink);
        (bsi, ContinueName ?? "").RunCancellable(ri =>
        {
            try
            {
                OnThen?.Invoke(this, ri);
            }
            finally
            {
                if (ri.IsRunning || ri.RepeatName == "") 
                    bsi.Flush();
                else 
                    GlobalLog.Instance?.Debug("Not flushing due to {rn} abort", ri.RepeatName);
            }
        });
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}