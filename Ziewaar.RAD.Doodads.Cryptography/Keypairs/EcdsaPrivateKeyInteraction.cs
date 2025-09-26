using Microsoft.DevTunnels.Ssh.Algorithms;

namespace Ziewaar.RAD.Doodads.Cryptography;

public class EcdsaPrivateKeyInteraction(
    IInteraction interaction,
    ECDsa.KeyPair ecdsa) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public ECDsa.KeyPair PrivateKey => ecdsa;
}