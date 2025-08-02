using System.Threading;

#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;
[Category("Flow Control")]
[Title("Delay and/or Repeat")]
[Description("""
       Will delay or intermittently repeat the underlying block.
       Be ware when using this with blocks that produce transient interactions, like HTTP servers.
       If the timer fires when the request has long ended, it will likely not do its work correctly,
       unless it's doing work that doesn't touch the transient response. Blocks that make transient
       interactions, are however expected to produce interactions that allow reading after the transient
       has ended. 
       """)]
public class Timer : IService, IDisposable
{
    private System.Threading.Timer? CurrentTimer;
    public void Dispose() => CurrentTimer?.Dispose();
    [PrimarySetting("""
                    Use a single number here to set both the delay and the interval in milliseconds.
                    Use two numbers separated with a pipe symbol '|' to set the interval and delay
                    respectively. Set either or both numbers to -1 to prevent the Timer from 
                    repeating, or starting at all, respectively.
                    """)]
    private readonly UpdatingPrimaryValue TimeSettingConstant = new();
    [NamedSetting("period", """
                            Sets the repeating rate in milliseconds. Defaults to 1 second. Use a 
                            negative value like -1 to prevent repeating
                            """)]
    private readonly UpdatingKeyValue PeriodConstant = new("period");
    [NamedSetting("due", """
                         Sets the initial delay in milliseconds. Defaults to 1 second. Use a
                         negative value like -1 to prevent starting
                         """)]
    private readonly UpdatingKeyValue DueConstant = new("due");
    private decimal Period, Due;
    [EventOccasion("Happens either on the repeating interval, or on the delay time.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("""
                   Happens when the Timer got a double command; ie. it was commanded to start,
                   and received a second start command. This prevents double firing.
                   """)]
    public event CallForInteraction? OnElse;
    [EventOccasion("Happens when the Timer wasn't preceeded by a Start or Stop command.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        ValidateConstants(constants);
        if (!interaction.TryGetClosest<TimerCommandInteraction>(out var candidateCommand, x => !x.IsConsumed) ||
            candidateCommand is not TimerCommandInteraction timerCommand)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "No command provided for timer"));
            return;
        }

        if (timerCommand.Command == TimerCommand.Start && this.CurrentTimer == null)
        {
            this.CurrentTimer = new System.Threading.Timer(_ =>
            {
                OnThen?.Invoke(this, interaction);
            }, null, Convert.ToInt64(this.Due), Convert.ToInt64(this.Period));
        } else if (timerCommand.Command == TimerCommand.Stop && this.CurrentTimer != null)
        {
            this.CurrentTimer.Dispose();
        }
        else
        {
            OnElse?.Invoke(this, interaction);
        }
    }
    private void ValidateConstants(StampedMap constants)
    {
        var periodNew = (constants, PeriodConstant).IsRereadRequired(() => 1000M, out Period);
        var dueNew = (constants, DueConstant).IsRereadRequired(() => 1000M, out Due);
        if ((constants, TimeSettingConstant).IsRereadRequired(() => { return $"{Period:0000}|{Due:0000}"; },
                out string? config))
        {
            var splitStr = config?.Split("|") ?? [];
            if (splitStr.Length >= 2)
            {
                Period = periodNew ? Period : Convert.ToDecimal(splitStr[0]);
                Due = dueNew ? Due : Convert.ToDecimal(splitStr[1]);
            } else if (splitStr.Length == 1)
            {
                Period = periodNew ? Period : Convert.ToDecimal(splitStr[0]);
                Due = dueNew ? Due : Convert.ToDecimal(splitStr[0]);
            }
            else
            {
                Period = periodNew ? Period : 1000M;
                Due = dueNew ? Due : 1000M;
            }
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}