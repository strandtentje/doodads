using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingMultipart;
public class TaggedReader(ICountingEnumerator<byte> ce) : ITaggedCountingEnumerator<byte>
{
    public bool MoveNext() => ce.MoveNext();
    public void Reset() => ce.Reset();
    byte IEnumerator<byte>.Current => ce.Current;
    object IEnumerator.Current => ce.Current;
    public void Dispose() => ce.Dispose();
    public bool AtEnd => ce.AtEnd;
    public long Cursor => ce.Cursor;
    public string? ErrorState { get => ce.ErrorState; set =>  ce.ErrorState = value; }
    public object Tag { get; set; } = string.Empty;
}