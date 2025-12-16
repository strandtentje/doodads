#nullable enable
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

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
    protected virtual bool IsRepeatNameRequired => true;
    protected abstract bool RunElse { get; }
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(
                out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (IsRepeatNameRequired && this.CurrentRepeatName == null)
        {
            OnException?.Invoke(this,
                interaction.AppendRegister("repeat name required"));
            return;
        }

        (interaction, this.CurrentRepeatName).RunCancellable(repeatInteraction =>
        {
            var items = GetItems(constants, repeatInteraction);
            repeatInteraction.IsRunning = true;
            var thenEnumerator = items.GetEnumerator();

            try
            {
                while (repeatInteraction.IsRunning && thenEnumerator.MoveNext())
                {
                    repeatInteraction.IsRunning = false;
                    if (thenEnumerator.Current != null)
                        OnThen?.Invoke(this, thenEnumerator.Current);
                }
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, interaction.AppendRegister(ex));
            }

            repeatInteraction.IsRunning &= RunElse;
            repeatInteraction.IsRunning |= OnElseRunningOverride;
            if (!repeatInteraction.IsRunning) return;

            var elseEnumerator = GetElseItems(constants, repeatInteraction).GetEnumerator();

            try
            {
                while (repeatInteraction.IsRunning && elseEnumerator.MoveNext())
                {
                    repeatInteraction.IsRunning = false;
                    if (elseEnumerator.Current != null)
                        OnElse?.Invoke(this, elseEnumerator.Current);
                }
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, interaction.AppendRegister(ex));
            }
        });
    }

    protected abstract IEnumerable<IInteraction> GetItems(StampedMap constants,
        IInteraction repeater);
    protected virtual bool OnElseRunningOverride => false;
    protected virtual IEnumerable<IInteraction> GetElseItems(StampedMap constants, IInteraction repeater)
    {
        return [];
    }

    public void HandleFatal(IInteraction source, Exception ex)
        => OnException?.Invoke(this, source);
}
