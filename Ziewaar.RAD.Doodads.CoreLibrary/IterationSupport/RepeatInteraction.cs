#nullable enable


namespace Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

public class RepeatInteraction(string repeatName, IInteraction parent) : IInteraction
{
    public string RepeatName => repeatName;
    public IInteraction Stack => parent;
    public bool IsDeep = false;
    public object Register => Stack.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public bool IsRunning = true;
    public IInteraction? ContinueFrom;

    public void Cancel()
    {
        IsRunning = false;
    }
}
