using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;
public class TaggedReader(ICountingEnumerator<byte> ce) : ITaggedCountingEnumerator<byte>
{
    public bool MoveNext() => ce.MoveNext();
    public void Reset() => ce.Reset();
    byte IEnumerator<byte>.Current => ce.Current;
    object? IEnumerator.Current => ce.Current;
    public void Dispose() => ce.Dispose();
    public bool AtEnd => ce.AtEnd;
    public long Cursor => ce.Cursor;
    public string? ErrorState { get => ce.ErrorState; set =>  ce.ErrorState = value; }
    public object Tag { get; set; }
}