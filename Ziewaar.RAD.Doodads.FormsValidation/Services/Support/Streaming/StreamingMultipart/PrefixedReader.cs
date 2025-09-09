using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class PrefixedReader(
    ICountingEnumerator<byte> byteSource,
    byte[] prefix) : ICountingEnumerator<byte>
{
    private readonly Queue<byte> PrefixQueue = new Queue<byte>(prefix);
    public byte Current { get; private set; }
    object? IEnumerator.Current => Current;
    public bool AtEnd { get; private set; }
    public long Cursor { get; private set; }
    public string? ErrorState { get; set; }

    public bool MoveNext()
    {
        while (PrefixQueue.TryDequeue(out byte item))
        {
            Current = item;
            Cursor++;
            return true;
        }

        while (byteSource.MoveNext())
        {
            Current = byteSource.Current;
            Cursor++;
            return true;
        }

        AtEnd = true;
        return false;
    }

    public void Reset()
    {
        AtEnd = false;
        Cursor = 0;
        ErrorState = null;
    }

    public void Dispose()
    {
    }
}