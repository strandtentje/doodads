#nullable enable
using Ejije.Logging;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public abstract class EventIteratingService<TArgs> : IteratingService
{
    protected abstract EventBlocker<TArgs>? CreateBlocker(StampedMap constants, IInteraction interaction);
    private static readonly Log
        DisposeFail = Log.Oops("Event Iterate Got {exception} while trying to dispose name");
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        using (var blocker = CreateBlocker(constants, repeater))
        {
            if (blocker == null) yield break;
            EventHandler<EventArgs> DisposalHandler = (s, e) =>
            {
                try
                {
                    blocker.Dispose();
                }
                catch (ObjectDisposedException)
                {

                }
                catch (Exception ex)
                {
                    Log.Post(DisposeFail, ex, (repeater as RepeatInteraction)?.RepeatName);
                }
            };
            this.InternalDisposeEvent += DisposalHandler;
            try
            {
                while (blocker.TryTake(out var args))
                {
                    if (args is (string key, object value)[] tupleArray)
                    {
                        yield return repeater.AppendMemory(tupleArray);
                    }
                    if (args != null)
                    {
                        yield return repeater.AppendRegister(args);
                    }
                    else
                    {
                        yield return repeater;
                    }
                }
            }
            finally
            {
                this.InternalDisposeEvent -= DisposalHandler;
            }
        }
    }
}
