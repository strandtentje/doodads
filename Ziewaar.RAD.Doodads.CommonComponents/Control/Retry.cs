using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control
{
    public class Retry : IteratingService, IDisposable
    {
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
}