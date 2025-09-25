using System.Collections.Concurrent;
using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingMultipart;

public class PrefixedReader(
    ICountingEnumerator<byte> byteSource,
    byte[] prefix) : ICountingEnumerator<byte>
{
    private readonly ConcurrentQueue<byte> PrefixQueue = new ConcurrentQueue<byte>(prefix);
    private byte _current;
    public byte Current => _current;
    object? IEnumerator.Current => _current;
    public bool AtEnd { get; private set; }
    private long _cursor;
    public long Cursor => _cursor;
    public string? ErrorState { get; set; }

    public bool MoveNext()
    {
        while (PrefixQueue.TryDequeue(out byte item))
        {
            _current = item;
            _cursor++;
            return true;
        }

        if (byteSource.MoveNext())
        {
            _current = byteSource.Current;
            _cursor++;
            return true;
        }

        AtEnd = true;
        return false;
    }

    public void Reset()
    {
        AtEnd = false;
        _cursor = 0;
        ErrorState = null;
    }

    public void Dispose()
    {
    }
}