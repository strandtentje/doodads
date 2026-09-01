#pragma warning disable 67
using Ejije.Logging;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

[Category("Sourcing & Sinking")]
[Title("Method-conditional Buffer Sink")]
[Description("""
             Works like sinkplug, but will only plug the sink if the HTTP method is in the 
             continue name.
             
             Use Continue to Flush; if no Continue is used, accumulated data will be rejected.
             """)]
public class MethodSinkPlug : IService
{
    [PrimarySetting("Continue-name for this service")]
    private readonly UpdatingPrimaryValue ContinueNameConstant = new();
    private string? ContinueName;
    [EventOccasion("New sink shows up here and buffers until control is returned to BufferSink")]
    public event CallForInteraction? OnThen;
    [EventOccasion("In case we didn't need to buffer because the method didn't match")] 
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when no original sink could be found")]
    public event CallForInteraction? OnException;
    private static readonly Log
        FlushAbort = Log.Tech("Not flushing due to {rn} abort");
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ContinueNameConstant).IsRereadRequired(out string? continueNameCandidate))
            this.ContinueName = continueNameCandidate;

        if (!interaction.TryGetClosest<IHttpHeadInteraction>(
            out var httpHead, httpHead => (this.ContinueName?.ToUpper() ?? "").Contains(httpHead.Method)))
        {
            OnElse?.Invoke(this, interaction);
            return;
        }

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
                    Log.Post(FlushAbort, ri.RepeatName);
            }
        });
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}