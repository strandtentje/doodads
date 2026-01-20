using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Rerun a job that died")]
[Description("""
             Combine this with a job that might die to re-start it.
             Pass Continue to the on them to indicate we keep on retrying.
             """)]
public class Retry : IteratingService, IDisposable
{
    [EventOccasion("Job to retry.")]
    public override event CallForInteraction? OnElse;
    private bool IsDisposed;
    protected override bool RunElse => false;

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        var retryInterval = Convert.ToInt32(constants.PrimaryConstant);
        
        while (!IsDisposed && !repeater.IsCancelled())
        {
            OnElse?.Invoke(this, repeater);
            yield return repeater;
            Thread.Sleep(retryInterval);
        }
    }

    public void Dispose()
    {
        this.IsDisposed = true;
    }
}