using Microsoft.DevTunnels.Ssh.Algorithms;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Keypairs.Services;

[Category("Tokens & Cryptography")]
[Title("Save a new ECDSA pair to PEM file")]
[Description("""Provided a filename to a new PEM in the Register, create an ECDSA keypair""")]
public class GenerateEcdsaPem : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var pemFileCandidate = interaction.Register.ToString();
        if (string.IsNullOrWhiteSpace(pemFileCandidate))
            OnException?.Invoke(this, new CommonInteraction(interaction, "Filename required"));
        else if (File.Exists(pemFileCandidate))
            OnException?.Invoke(this, new CommonInteraction(interaction, "Cannot overwrite PEM"));
        else if (!Directory.Exists(Path.GetDirectoryName(pemFileCandidate)))
            OnException?.Invoke(this, new CommonInteraction(interaction, "Cannot write pem to non-existent dir."));
        else
        {
            var newPemFile = new FileInfo(pemFileCandidate);
            var tunnelEcdsaPair = ECDsa.KeyPair.Generate(ECDsa.ECDsaSha2Nistp256);
            var netEcdsa = System.Security.Cryptography.ECDsa.Create();
            netEcdsa.ImportParameters(tunnelEcdsaPair.ExportParameters(includePrivate: true));
            File.WriteAllText(newPemFile.FullName, netEcdsa.ExportPkcs8PrivateKeyPem());
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}