using System.Text;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public static class DebugByteString
{
    public static string ToEscapedAscii(IEnumerable<byte> bytes)
    {
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            if (b >= 0x20 && b <= 0x7E) // Printable ASCII
            {
                sb.Append((char)b);
            }
            else
            {
                // Use escape sequences for common control characters
                sb.Append(b switch
                {
                    0x00 => @"\0",
                    0x07 => @"\a",
                    0x08 => @"\b",
                    0x09 => @"\t",
                    0x0A => @"\n",
                    0x0B => @"\v",
                    0x0C => @"\f",
                    0x0D => @"\r",
                    _ => $@"\x{b:X2}"
                });
            }
        }
        return sb.ToString();
    }
}