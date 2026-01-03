using System.Collections.Concurrent;
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Enter the populating of a particular zip name")]
[Description("""
             Use this between ZipIn and ZipOut to specify to which name we'll be enqueueing. 
             OnThen is spun up concurrently, and when its done doing work, ZipEnter the Zip sink
             as complete such that ZipLine won't expect more items.
             """)]
public class ZipSource : IService
{
    [EventOccasion("Runs concurrently to start populating the zip list")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("zip name wasn't in register, or wasn't found.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ZipInteraction>(out var zi)
            || zi == null
            || interaction.Register.ToString() is not { } zipName
            || zi.MergeCollections.Any(x => x.Name == zipName))
        {
            OnException?.Invoke(this, interaction.AppendRegister("previously unregistered zip name expected in register"));
            return;
        }
        var bc = new BlockingCollection<IInteraction>();
        zi.MergeCollections.Add((zipName, bc));
        ThreadPool.QueueUserWorkItem(_ =>
        {
            OnThen?.Invoke(this, new ZipFocusInteraction(interaction, bc));
            bc.CompleteAdding();
        }, null);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}