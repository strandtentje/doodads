using System.Threading;
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control
{
    public class TimerService : IService, IDisposable
    {
        private Timer? CurrentTimer;
        public void Dispose() => CurrentTimer?.Dispose();
        private readonly UpdatingPrimaryValue TimeSettingConstant = new();
        private readonly UpdatingKeyValue PeriodConstant = new("period");
        private readonly UpdatingKeyValue DueConstant = new("due");
        private decimal Period, Due;
        public event CallForInteraction? OnThen;
        public event CallForInteraction? OnElse;
        public event CallForInteraction? OnException;
        public void Enter(StampedMap constants, IInteraction interaction)
        {
            ValidateConstants(constants);
            if (!interaction.TryGetClosest<TimerCommandInteraction>(out var candidateCommand, x => x.IsConsumed) ||
                candidateCommand is not TimerCommandInteraction timerCommand)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "No command provided for timer"));
                return;
            }

            if (timerCommand.Command == TimerCommand.Start && this.CurrentTimer == null)
            {
                this.CurrentTimer = new Timer(_ =>
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
            if ((constants, TimeSettingConstant).IsRereadRequired(() => { return $"{Period:0000}-{Due:0000}"; },
                    out string? config))
            {
                var splitStr = config?.Split("-") ?? [];
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
}
