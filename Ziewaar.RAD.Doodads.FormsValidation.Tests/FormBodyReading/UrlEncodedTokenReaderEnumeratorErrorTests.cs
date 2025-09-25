using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.FormBodyReading;
[TestClass]
public class UrlEncodedTokenReaderEnumeratorErrorTests
{
    private static ICountingEnumerator<byte> FromAscii(string s)
        => new RootByteReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(s)), limit: -1);

    private static string Drain(ICountingEnumerator<char> r)
    {
        var sb = new System.Text.StringBuilder();
        while (r.CanContinue() && r.MoveNext()) sb.Append(r.Current);
        return sb.ToString();
    }

    [TestMethod]
    public void MissingAmpersand_BetweenPairs_IsError()
    {
        // "a=1b=2" (no '&' between pairs) – should error when trying to advance to second pair
        var tok = new UrlEncodedTokenReader(FromAscii("a=1b=2"));

        Assert.IsTrue(tok.MoveNext());
        Assert.AreEqual("a", tok.Current.Key);
        Assert.AreEqual("1b", Drain(tok.Current.Value));

        Assert.IsFalse(tok.MoveNext(), "Expected failure due to missing '&' delimiter.");
        Assert.AreEqual("Expected ampersand midway in form", tok.ErrorState);
    }

    [TestMethod]
    public void UnsafeChar_In_Key_IsError()
    {
        // '[' is unsafe in strict mode; must be percent-encoded
        var tok = new UrlEncodedTokenReader(FromAscii("a[0]=1"));
        Assert.IsFalse(tok.MoveNext(), "Unsafe key char should error.");
        Assert.IsFalse(string.IsNullOrEmpty(tok.ErrorState));
    }

    [TestMethod]
    public void MalformedPercent_In_Key_Or_Value_IsError()
    {
        var badKey = new UrlEncodedTokenReader(FromAscii("%G1=ok"));
        Assert.IsFalse(badKey.MoveNext());
        Assert.IsFalse(string.IsNullOrEmpty(badKey.ErrorState));

        var badVal = new UrlEncodedTokenReader(FromAscii("a=%E2%82")); // truncated UTF-8 escape
        Assert.IsTrue(badVal.MoveNext()); // positioned on pair
        var vr = badVal.Current.Value;
        Assert.IsFalse(vr.MoveNext(), "Value reader should set error on truncated percent escape.");
        Assert.IsFalse(string.IsNullOrEmpty(vr.ErrorState));
    }

    [TestMethod]
    public void KeyLengthLimit_IsEnforced_OnDecodedChars()
    {
        var longKey = new string('k', 250);
        var tok = new UrlEncodedTokenReader(FromAscii(longKey + "=v"), keyLengthLimit: 200);

        Assert.IsFalse(tok.MoveNext(), "Key longer than limit should fail.");
        Assert.IsFalse(string.IsNullOrEmpty(tok.ErrorState));
    }
}