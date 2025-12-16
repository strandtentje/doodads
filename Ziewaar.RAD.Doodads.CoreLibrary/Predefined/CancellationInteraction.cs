#nullable enable

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
#pragma warning disable 67

public class CancellationInteraction(IInteraction parent, string name) : IInteraction
{
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public event EventHandler? Cancelled;
    public string Name => name;
    public void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);
}
