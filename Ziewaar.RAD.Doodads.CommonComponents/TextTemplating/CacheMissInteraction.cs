#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

public class CacheMissInteraction(IInteraction stack, IEnumerable<object> validateKeys, CacheKey key, CacheValue value)
    : IInteraction
{
    public IInteraction Stack => stack;
    public object Register => stack.Register;
    public IReadOnlyDictionary<string, object> Memory => stack.Memory;
    public IEnumerable<object> ValidateKeys => validateKeys;
    public CacheKey Key => key;
    public CacheValue Value => value;
}