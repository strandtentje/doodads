#nullable enable
using Serilog.Core;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public abstract class IteratingService : IService, IDisposable
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    protected string? CurrentRepeatName;
    protected bool IsDisposing;

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
            using (var thenEnumerator = items.GetEnumerator())
            {
                void HandleLocalDispose(object? sender, EventArgs e)
                {
                    try
                    {
                        thenEnumerator.Dispose();
                    } catch(ObjectDisposedException)
                    {

                    } catch(Exception ex)
                    {
                        GlobalLog.Instance?.Warning(ex, "while trying to dispose then-enumerator of {name}", this.CurrentRepeatName);
                    }
                }

                this.InternalDisposeEvent += HandleLocalDispose;

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
                } finally
                {
                    this.InternalDisposeEvent -= HandleLocalDispose;
                }

            }

            repeatInteraction.IsRunning &= RunElse;
            repeatInteraction.IsRunning |= OnElseRunningOverride;
            if (!repeatInteraction.IsRunning) return;

            using (var elseEnumerator = GetElseItems(constants, repeatInteraction).GetEnumerator())
            {
                void HandleLocalDispose(object? sender, EventArgs e)
                {
                    try
                    {
                        elseEnumerator.Dispose();
                    } catch(ObjectDisposedException)
                    {

                    } catch(Exception ex)
                    {
                        GlobalLog.Instance?.Warning(ex, "while trying to dispose else-enumerator of {name}", this.CurrentRepeatName);
                    }
                }

                this.InternalDisposeEvent += HandleLocalDispose;

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
                finally
                {
                    this.InternalDisposeEvent -= HandleLocalDispose;
                }
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
    protected event EventHandler<EventArgs>? InternalDisposeEvent;
    public virtual void Dispose()
    {
        this.IsDisposing = true;
        InternalDisposeEvent?.Invoke(this, EventArgs.Empty);
    }
}
