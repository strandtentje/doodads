#nullable enable
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class ScopedEventInteraction(IInteraction parent, EventWaitHandle handle) : IInteraction
{
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public EventWaitHandle Ewh => handle;    
}
