#nullable enable


using System.Collections.Concurrent;
using System.Threading;

namespace Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

public class RepeatInteraction(string repeatName, IInteraction parent, CancellationToken cancellationToken)
    : IInteraction
{
    public string RepeatName => repeatName;
    public IInteraction Stack => parent;
    public bool IsDeep = false;
    public object Register => Stack.Register;
    public int GetCounterValue() => counterMemory.GetCounterValue();
    private readonly RepeatInteractionCounterMemory counterMemory = new RepeatInteractionCounterMemory(repeatName);
    public IReadOnlyDictionary<string, object> Memory => counterMemory;
    public CancellationToken CancellationToken => cancellationToken;
    public bool IsRunning
    {
        get
        {
            return field && !CancellationToken.IsCancellationRequested;
        }
        set
        {
            if (value != field && value == true)
            {
                counterMemory.Increment();
            }
            field = value;
        }
    }
    public IInteraction? ContinueFrom;

    public void Cancel()
    {
        IsRunning = false;
    }
}
