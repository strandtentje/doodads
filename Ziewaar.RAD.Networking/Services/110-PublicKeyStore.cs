using Serilog;
using Ziewaar.Network.Encryption;

namespace Ziewaar.RAD.Networking;

public class PublicKeyStore : KeyStore<PublicKeyRepository>
{
    protected override PublicKeyRepository Create(ILogger logger, string keyDir) => new(logger, keyDir);
}