using System.Security.Cryptography;
using System.Text;

namespace Ziewaar.RAD.Doodads.Cryptography;

public static class RsaExtensions
{
    // Encryption
    public static byte[] EncryptData(this RSA rsa, byte[] data)
        => rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);

    public static byte[] EncryptString(this RSA rsa, string text)
        => rsa.EncryptData(Encoding.UTF8.GetBytes(text));

    // Decryption
    public static byte[] DecryptData(this RSA rsa, byte[] encrypted)
        => rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);

    public static string DecryptToString(this RSA rsa, byte[] encrypted)
        => Encoding.UTF8.GetString(rsa.DecryptData(encrypted));

    // Signing
    public static byte[] SignDataSha256(this RSA rsa, byte[] data)
        => rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    public static byte[] SignStringSha256(this RSA rsa, string text)
        => rsa.SignDataSha256(Encoding.UTF8.GetBytes(text));

    // Verification
    public static bool VerifyDataSha256(this RSA rsa, byte[] data, byte[] signature)
        => rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    public static bool VerifyStringSha256(this RSA rsa, string text, byte[] signature)
        => rsa.VerifyDataSha256(Encoding.UTF8.GetBytes(text), signature);

    // Export/Import PEM
    public static string ExportPublicKeyPem(this RSA rsa)
        => rsa.ExportSubjectPublicKeyInfoPem();

    public static string ExportPrivateKeyPem(this RSA rsa)
        => rsa.ExportPkcs8PrivateKeyPem();
}



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
