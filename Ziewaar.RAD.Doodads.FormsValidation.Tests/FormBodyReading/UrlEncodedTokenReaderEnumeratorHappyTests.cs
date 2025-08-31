using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.FormBodyReading;
[TestClass]
public class UrlEncodedTokenReaderEnumeratorHappyTests
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
    public void SinglePair_WithValue_IsProduced_AndValueMustBeFullyRead()
    {
        var tok = new UrlEncodedTokenReader(FromAscii("a=1"));

        // first pair
        Assert.IsTrue(tok.MoveNext(), "First MoveNext should position on first pair.");
        Assert.AreEqual("a", tok.Current.Key);

        // value present and readable
        var v = tok.Current.Value;
        Assert.AreEqual("1", Drain(v));

        // end
        Assert.IsFalse(tok.MoveNext(), "No more pairs.");
    }

    [TestMethod]
    public void TwoPairs_StandardCase()
    {
        var tok = new UrlEncodedTokenReader(FromAscii("a=1&b=2"));

        Assert.IsTrue(tok.MoveNext());
        Assert.AreEqual("a", tok.Current.Key);
        Assert.AreEqual("1", Drain(tok.Current.Value));

        Assert.IsTrue(tok.MoveNext());
        Assert.AreEqual("b", tok.Current.Key);
        Assert.AreEqual("2", Drain(tok.Current.Value));

        Assert.IsFalse(tok.MoveNext());
    }

    [TestMethod]
    public void EmptyValue_WithEquals_And_EmptyValue_NoEquals()
    {
        var tok = new UrlEncodedTokenReader(FromAscii("x=&y"));

        Assert.IsTrue(tok.MoveNext());
        Assert.AreEqual("x", tok.Current.Key);
        Assert.AreEqual("", Drain(tok.Current.Value));

        Assert.IsTrue(tok.MoveNext());
        Assert.AreEqual("y", tok.Current.Key);
        Assert.AreEqual("", Drain(tok.Current.Value));

        Assert.IsFalse(tok.MoveNext());
    }

    [TestMethod]
    public void PlusAndPercentDecoding_InKey_And_Value()
    {
        // key "a b" encoded as a+b ; value "€ ok" encoded as %E2%82%AC+ok
        var tok = new UrlEncodedTokenReader(FromAscii("a+b=%E2%82%AC+ok"));

        Assert.IsTrue(tok.MoveNext());
        Assert.AreEqual("a b", tok.Current.Key);
        Assert.AreEqual("€ ok", Drain(tok.Current.Value));
        Assert.IsFalse(tok.MoveNext());
    }

    [TestMethod]
    public void EqualsConsumption_NoFirstValueByteLost()
    {
        var tok = new UrlEncodedTokenReader(FromAscii("k=XY"));

        Assert.IsTrue(tok.MoveNext());
        Assert.AreEqual("k", tok.Current.Key);
        Assert.AreEqual("XY", Drain(tok.Current.Value)); // if this fails, '=' wasn’t consumed before creating the value reader
        Assert.IsFalse(tok.MoveNext());
    }

    [TestMethod]
    public void AmpersandAfterEmptyValue_IsConsumed_AndPositionsAtNextKey()
    {
        var tok = new UrlEncodedTokenReader(FromAscii("a&b=1"));

        Assert.IsTrue(tok.MoveNext());
        Assert.AreEqual("a", tok.Current.Key);
        Assert.AreEqual("", Drain(tok.Current.Value)); // empty value

        Assert.IsTrue(tok.MoveNext());                // must now be on 'b'
        Assert.AreEqual("b", tok.Current.Key);
        Assert.AreEqual("1", Drain(tok.Current.Value));

        Assert.IsFalse(tok.MoveNext());
    }

    [TestMethod]
    public void MustFullyRead_PreviousValue_Before_MoveNext()
    {
        var tok = new UrlEncodedTokenReader(FromAscii("a=123&b=ok"));

        Assert.IsTrue(tok.MoveNext());
        Assert.AreEqual("a", tok.Current.Key);

        // read only one char from the value
        var vr = tok.Current.Value;
        Assert.IsTrue(vr.MoveNext());
        Assert.AreEqual('1', vr.Current);

        // now MoveNext should refuse to advance because previous value wasn’t fully read
        Assert.IsFalse(tok.MoveNext(), "MoveNext should fail when previous value reader wasn’t drained.");
        Assert.IsFalse(string.IsNullOrEmpty(tok.ErrorState), "Reader should set an error on premature MoveNext.");
    }
}