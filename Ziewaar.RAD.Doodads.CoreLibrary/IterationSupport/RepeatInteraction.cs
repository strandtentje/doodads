#nullable enable


namespace Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

public class RepeatInteraction(string repeatName, IInteraction parent)
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
    private bool LastRunningState = true;
    private int Counter = 0;
    public bool IsRunning
    {
        get
        {
            return LastRunningState;
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