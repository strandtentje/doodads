using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.Cryptography.Secrets.Interactions;
public class ExpiringStringInteraction(IInteraction stack, string overrideName, string sensitiveValue) : ISensitiveInteraction
{
    public IInteraction Stack => stack;
    public object Register => stack.Register;
    public IReadOnlyDictionary<string, object> Memory =>
        new SwitchingDictionary([overrideName], _ => "");
    private readonly Lock Sensitivity = new();
    private bool IsVirgin = true;
    public object GetSensitiveObject()
    {
        lock (Sensitivity)
        {
            if (IsVirgin)
            {
                IsVirgin = false;
                return sensitiveValue;
            }
            throw new StringExpiredException(overrideName);
        }
    }
    public bool TryVirginity()
    {
        lock (Sensitivity)
        {
            return IsVirgin;
        }
    }
}