using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.Cryptography;

[Category("Tokens & Cryptography")]
[Title("Save a new ECDSA pair to PEM file")]
[Description("""Provided an ECDSA keypair, extract the public key PEM.""")]
public class GetEcdsaPublicKey : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<EcdsaPrivateKeyInteraction>(out EcdsaPrivateKeyInteraction? privateKey) ||
            privateKey == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "private key required to extract public key from."));
            return;
        }
        else
        {
            var netEcdsaPair = System.Security.Cryptography.ECDsa.Create();
            netEcdsaPair.ImportParameters(privateKey.PrivateKey.ExportParameters(includePrivate: false));
            OnThen?.Invoke(this, new CommonInteraction(interaction, netEcdsaPair.ExportSubjectPublicKeyInfoPem()));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}