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
    private readonly SortedList<string, object> _sorted = new(1)
    {
        [$"Number of {repeatName}"] = 0
    };
    public IReadOnlyDictionary<string, object> Memory => _sorted;
    public CancellationToken CancellationToken => cancellationToken;
    private bool LastRunningState = true;
    private int Counter = 0;
    public bool IsRunning
    {
        get
        {
            return LastRunningState && !CancellationToken.IsCancellationRequested;
        }
        set
        {
            if (value != LastRunningState && value == true)
            {
                Counter++;
                _sorted[$"Number of {repeatName}"] = Counter;
            }
            LastRunningState = value;
        }
    }
    public IInteraction? ContinueFrom;
    
    public void Cancel()
    {
        IsRunning = false;
    }
}