namespace Ziewaar.RAD.Doodads.Cryptography;
public interface ISensitiveInteraction : IInteraction
{
    object GetSensitiveObject();
    bool TryVirginity();
}