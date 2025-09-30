namespace Ziewaar.RAD.Doodads.Cryptography.Keypairs;
public static class RsaCryptoFactory
{
    // Key generation
    public static RSA GenerateKeyPair(int keySize = 2048)
        => RSA.Create(keySize);

    // Save keys
    public static void SavePublicKeyPem(RSA rsa, string path)
        => File.WriteAllText(path, rsa.ExportPublicKeyPem());

    public static void SavePrivateKeyPem(RSA rsa, string path)
        => File.WriteAllText(path, rsa.ExportPrivateKeyPem());

    // Load keys
    public static RSA LoadPrivateKeyPem(string path)
    {
        var pem = File.ReadAllText(path);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem.ToCharArray());
        return rsa;
    }

    public static RSA LoadPublicKeyPem(string path)
    {
        var pem = File.ReadAllText(path);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem.ToCharArray());
        return rsa;
    }
}