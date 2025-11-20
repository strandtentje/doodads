#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
#pragma warning disable 67
public abstract class IteratingService : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    [EventOccasion("Next item")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public virtual event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when there was no repeat name")]
    public virtual event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(
                out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (this.CurrentRepeatName == null)
        {
            OnException?.Invoke(this,
                interaction.AppendRegister("repeat name required"));
            return;
        }

        var repeatInteraction =
            new RepeatInteraction(this.CurrentRepeatName, interaction);
        var items = GetItems(constants, repeatInteraction);
        repeatInteraction.IsRunning = true;
        var enumerator = items.GetEnumerator();
        try
        {
            while (repeatInteraction.IsRunning && enumerator.MoveNext())
            {
                repeatInteraction.IsRunning = false;
                if (enumerator.Current != null)
                    OnThen?.Invoke(this, enumerator.Current);
            }
        }
        catch (Exception ex)
        {
            OnException?.Invoke(this, interaction.AppendRegister(ex));
        }
        finally
        {
            enumerator.Dispose();
        }
    }

    protected abstract IEnumerable<IInteraction> GetItems(StampedMap constants,
        IInteraction repeater);

    public void HandleFatal(IInteraction source, Exception ex)
        => OnException?.Invoke(this, source);
}
