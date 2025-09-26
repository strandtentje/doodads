using System.Security.Cryptography;

namespace Ziewaar.RAD.Doodads.Cryptography.Keypairs;
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