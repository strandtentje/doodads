#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

public class CacheHitInteraction(IInteraction stack, CacheKey key, CacheValue value) : IInteraction
{
    public IInteraction Stack => stack;
    public object Register => stack.Register;
    public IReadOnlyDictionary<string, object> Memory => stack.Memory;
    public CacheKey Key => key;
    public CacheValue Value => value;
}