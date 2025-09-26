using Microsoft.DevTunnels.Ssh.Algorithms;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.Cryptography;

[Category("Tokens & Cryptography")]
[Title("Load ECDSA pair from PEM file")]
[Description("""Provided a filename to a PEM in the Register, recover an ECDSA keypair""")]
public class LoadEcdsaPem : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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