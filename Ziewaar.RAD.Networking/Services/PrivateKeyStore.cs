using Serilog;
using Ziewaar.Network.Encryption;

namespace Ziewaar.RAD.Networking;

public class PrivateKeyStore : KeyStore<PrivateKeyRepository>
{
    protected override PrivateKeyRepository Create(ILogger logger, string keyDir) => new(logger, keyDir);
}