using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Zip multiple emitting services")]
[Description("""
             Use this with ZipOut, ZipExit and ZipLine to combine the output of multiple concurrent services
             into one one interaction. ZipIn is used to start the zipping, which means it defines which zip inputs
             are available by name. It'll continue outputting according to IteratingService rules ie with Continue("")
              or [""]
             """)]
[ShortNames("zi")]
public class ZipIn : IteratingService
{
    [EventOccasion("Zip sources")]
    public override event CallForInteraction? OnElse;
    protected override bool RunElse { get; } = false;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        var mergeCollections = new List<(string, BlockingCollection<IInteraction>)>();
        var zi = new ZipInteraction(repeater, mergeCollections);
        OnElse?.Invoke(this, zi);

        if (zi.MergeCollections.Count == 0)
            throw new Exception("No zip sources were set; use ZipSource.");

        do
        {
            yield return zi;
        } while (zi.MergeCollections.Any(x => !x.Collection.IsCompleted));
    }
}