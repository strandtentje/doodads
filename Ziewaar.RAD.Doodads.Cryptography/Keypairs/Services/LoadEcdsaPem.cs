using ECDsa = Microsoft.DevTunnels.Ssh.Algorithms.ECDsa;

#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Keypairs.Services;

[Category("Tokens & Cryptography")]
[Title("Load ECDSA pair from PEM file")]
[Description("""Provided a filename to a PEM in the Register, recover an ECDSA keypair""")]
public class LoadEcdsaPem : IService
{
    [EventOccasion("The loaded ECDSA pub/priv keypair is available here for the services that can use it.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when the file didn't exist.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var pemFileCandidate = interaction.Register.ToString();
        if (!File.Exists(pemFileCandidate))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "PEM didn't exist."));
            return;
        }

        var netEcdsaPair = System.Security.Cryptography.ECDsa.Create();
        netEcdsaPair.ImportFromPem(File.ReadAllText(pemFileCandidate));
        var tunnelEcdsaPair = new ECDsa.KeyPair();
        tunnelEcdsaPair.ImportParameters(netEcdsaPair.ExportParameters(includePrivateParameters: true));

        OnThen?.Invoke(this, new EcdsaPrivateKeyInteraction(interaction, tunnelEcdsaPair));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}