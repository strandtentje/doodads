using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control
{
    public class TimerService : IService, IDisposable
    {
        private Timer CurrentTimer;
        [NamedBranch]
        public event EventHandler<IInteraction> OnError;
        [NamedBranch]
        public event EventHandler<IInteraction> Continue;
        public void Dispose() => CurrentTimer?.Dispose();
        public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
        {
            if (!interaction.TryGetClosest<TimerCommandInteraction>(out var candidateCommand, x => x.IsConsumed) || 
                candidateCommand is not TimerCommandInteraction timerCommand)
                return;

            if (timerCommand.Command == TimerCommand.Start && this.CurrentTimer == null)
            {
                var stringPeriod = serviceConstants.InsertIgnore<string>("period", "5.5");
                var stringDue = serviceConstants.InsertIgnore<string>("due", "5.5");
                var decmialPeriod = decimal.TryParse(stringPeriod, out decimal candidatePeriod) ? candidatePeriod : decimal.MaxValue;
                var decimalDue = decimal.TryParse(stringDue, out decimal candidateDue) ? candidateDue : decimal.MaxValue;
                var tsPeriod = TimeSpan.FromSeconds((double)decmialPeriod);
                var tsDue = TimeSpan.FromSeconds((double)decimalDue);

                this.CurrentTimer = new Timer(_ =>
                {
                    Continue?.Invoke(this, interaction);
                }, null, tsDue, tsPeriod);                
            } else if (timerCommand.Command == TimerCommand.Stop && this.CurrentTimer != null)
            {
                this.CurrentTimer.Dispose();
            }
        }
    }
}
