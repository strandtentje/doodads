#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public abstract class EventIteratingService<TArgs> : IteratingService
{
    protected abstract EventBlocker<TArgs>? CreateBlocker(StampedMap constants, IInteraction interaction);
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        using var blocker = CreateBlocker(constants, repeater);
        if (blocker == null) yield break;
        while(blocker.TryTake(out var args))
        {            
            if (args is (string key, object value)[] tupleArray)
            {
                yield return repeater.AppendMemory(tupleArray);
            }
            if (args != null)
            {
                yield return repeater.AppendRegister(args);
            } else
            {
                yield return repeater;
            }
        }
    }
}
