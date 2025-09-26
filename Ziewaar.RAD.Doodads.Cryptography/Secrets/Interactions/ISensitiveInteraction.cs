namespace Ziewaar.RAD.Doodads.Cryptography.Secrets.Interactions;
public interface ISensitiveInteraction : IInteraction
{
    object GetSensitiveObject();
    bool TryVirginity();
}