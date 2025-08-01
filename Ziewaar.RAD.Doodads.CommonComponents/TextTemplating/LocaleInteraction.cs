#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
public class LocaleInteraction(IInteraction stack, string localeString) : IInteraction
{
    public IInteraction Stack => stack;
    public object Register => stack.Register;
    public IReadOnlyDictionary<string, object> Memory => stack.Memory;
    public string Locale => localeString;
}