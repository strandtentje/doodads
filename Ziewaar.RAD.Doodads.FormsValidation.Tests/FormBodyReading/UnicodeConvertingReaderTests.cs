using System.Text;
using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.FormBodyReading;
[TestClass]
public class UnicodeConvertingReaderTests
{
    private static ICountingEnumerator<char> Uni(params byte[] utf8)
        => new UnicodeConvertingReader(new RootByteReader(new MemoryStream(utf8), limit: -1), limit: -1);

    private static string ReadAll(ICountingEnumerator<char> r)
    {
        var sb = new System.Text.StringBuilder();
        while (r.MoveNext()) sb.Append(r.Current);
        return sb.ToString();
    }

    [TestMethod]
    public void Decodes_Ascii_And_Multibyte()
    {
        // "A" U+0041
        Assert.AreEqual("A", ReadAll(Uni(0x41)));

        // "¢" U+00A2 => C2 A2
        Assert.AreEqual("¢", ReadAll(Uni(0xC2, 0xA2)));

        // "€" U+20AC => E2 82 AC
        Assert.AreEqual("€", ReadAll(Uni(0xE2, 0x82, 0xAC)));
/*
        // "😀" U+1F600 => F0 9F 98 80
        var smile = "😀";
        var smileChars = smile.ToArray();
        var uc = Encoding.UTF8.GetBytes(smile);
        var chr = Encoding.UTF8.GetChars(uc);
        Assert.AreEqual(smile, ReadAll(Uni(uc)));*/
    }

    [TestMethod]
    public void InvalidUtf8_SetsError()
    {
        // Lone continuation byte 0x80
        var r = Uni(0x80);
        Assert.IsFalse(r.MoveNext());
        Assert.IsFalse(string.IsNullOrEmpty(r.ErrorState));

        // Truncated multi-byte
        r = Uni(0xE2, 0x82); // missing last byte
        Assert.IsFalse(r.MoveNext());
        Assert.IsFalse(string.IsNullOrEmpty(r.ErrorState));
    }
}