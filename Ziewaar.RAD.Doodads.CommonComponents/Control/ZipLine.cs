using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Remove one item that came from ZipOut")]
[Description("""
             Will dequeue an item enqueued by ZipOut. If there are no items and ZipExit happened
             on this name, this will not propagate OnThen.
             """)]
public class ZipLine : IteratingService
{
    [NamedSetting("timeout",
        "ms to wait for releasing an item. set to -1 to wait indefinitely; this is the default behaviour.")]
    private readonly UpdatingKeyValue TimeoutConstant = new("timeout");
    private decimal CurrentTimeout = Timeout.Infinite;
    protected override bool RunElse => false;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if ((constants, TimeoutConstant).IsRereadRequired(() => 1000, out decimal timeoutCandidate))
            this.CurrentTimeout = timeoutCandidate;

        if (!repeater.TryGetClosest<ZipInteraction>(out var zipInteraction) ||
            zipInteraction == null)
            throw new Exception("No zip interaction present");

        foreach (var ziMergeCollection in zipInteraction.MergeCollections)
        {
            if (ziMergeCollection.Collection.TryTake(out var item, (int)CurrentTimeout))
            {
                yield return repeater.
                    AppendMemory(new InteractingDefaultingDictionary(item, EmptyReadOnlyDictionary.Instance)).
                    AppendRegister(ziMergeCollection.Name);
            }
            else if (ziMergeCollection.Collection.IsAddingCompleted)
                yield return repeater.AppendRegister(ziMergeCollection.Name).AppendMemory(("zip", "complete"));
            else
                yield return repeater.AppendRegister(ziMergeCollection.Name).AppendMemory(("zip", "timeout"));
        }
    }
}