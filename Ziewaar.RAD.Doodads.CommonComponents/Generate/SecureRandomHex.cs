#pragma warning disable 67
#nullable enable
using System.Security.Cryptography;
using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Generate;

public static class SecureRandomHex
{

    private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

    public static string Generate(int hexLength = 48)
    {
        if (hexLength % 2 != 0)
            throw new ArgumentException("hexLength must be even (2 hex chars per byte).", nameof(hexLength));

        int byteLength = hexLength / 2;
        byte[] data = new byte[byteLength];
        rng.GetBytes(data);
        var sb = new StringBuilder(hexLength);
        foreach (byte b in data)
            sb.AppendFormat("{0:x2}", b);

        return sb.ToString();
    }
}

