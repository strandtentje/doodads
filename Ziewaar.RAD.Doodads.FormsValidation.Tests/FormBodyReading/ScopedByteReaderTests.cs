using System.Collections;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.FormBodyReading;
[TestClass]
public class ScopedByteReaderTests
{
    private sealed class Seq : ICountingEnumerator<byte>
    {
        private readonly byte[] _data;
        public Seq(params byte[] data) { _data = data; }
        public bool AtEnd { get; private set; }
        public long Cursor { get; private set; }
        public string? ErrorState { get; set; }
        public byte Current { get; private set; }
        object IEnumerator.Current => Current;
        public void Dispose() { }
        public bool MoveNext()
        {
            if (AtEnd) return false;
            if (Cursor >= _data.Length) { AtEnd = true; return false; }
            Current = _data[Cursor++];
            return true;
        }
        public void Reset() { throw new NotSupportedException(); }
    }

    [TestMethod]
    public void YieldsUntilTerminator_ThenStops()
    {
        var inner = new Seq((byte)'A', (byte)'B', (byte)'C', (byte)'&', (byte)'D');
        var scoped = new ScopedByteReader("abc", inner, limit: -1, (byte)'&');

        var got = new List<byte>();
        while (scoped.MoveNext()) got.Add(scoped.Current);

        CollectionAssert.AreEqual(new byte[] { (byte)'A', (byte)'B', (byte)'C' }, got);
        Assert.IsTrue(scoped.AtEnd);
        Assert.IsNull(scoped.ErrorState);
    }

    [TestMethod]
    public void EnforcesLimit()
    {
        var inner = new Seq((byte)'1', (byte)'2', (byte)'3', (byte)'4');
        var scoped = new ScopedByteReader("limited", inner, limit: 2, (byte)'&');

        var got = new List<byte>();
        while (scoped.MoveNext()) got.Add(scoped.Current);

        CollectionAssert.AreEqual(new byte[] { (byte)'1', (byte)'2' }, got);
        Assert.IsFalse(string.IsNullOrEmpty(scoped.ErrorState));
    }
}