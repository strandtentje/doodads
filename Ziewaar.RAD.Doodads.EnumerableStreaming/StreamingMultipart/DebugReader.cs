using System.Collections.Concurrent;
using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingMultipart;

public class DebugReader(ICountingEnumerator<byte> OrignalItems, Action<(string hindisght, string cursor, string foresight)> callback) : ICountingEnumerator<byte>
{
    private const int CAPACITY = 20;
    private readonly ConcurrentQueue<byte> Foresight = new ConcurrentQueue<byte>();
    private readonly BoundedQueue<byte> Hindsight = new BoundedQueue<byte>(CAPACITY*2);
    public byte Current { get; private set; }
    object? IEnumerator.Current => Current;
    public bool AtEnd { get; private set; }
    public long Cursor { get; private set; }
    public string? ErrorState { get => OrignalItems.ErrorState; set =>  OrignalItems.ErrorState = value; }

    public bool MoveNext()
    {
        try
        {
            if (!AtEnd && Current > 0 && Cursor > 0)
                Hindsight.Enqueue(Current);
            if (!AtEnd && Current == 0 && Cursor > 0)
            {
                Hindsight.Enqueue((byte)'\\');
                Hindsight.Enqueue((byte)'0');
            }

            while (Foresight.Count < CAPACITY && OrignalItems.MoveNext())
                Foresight.Enqueue(OrignalItems.Current);
            if (Foresight.TryDequeue(out byte retrievedItem))
            {
                this.Current = retrievedItem;
                Cursor++;
                return true;
            }

            AtEnd = true;
            return false;
        }
        finally
        {
            callback((DebugByteString.ToEscapedAscii(Hindsight),
                DebugByteString.ToEscapedAscii([Current]), DebugByteString.ToEscapedAscii(Foresight)));
        }
    }

    public void Reset()
    {
        AtEnd = false;
        Cursor = 0;
        Hindsight.Clear();
        while (Foresight.TryDequeue(out var _)) ;
        OrignalItems.Reset();
    }

    public void Dispose() => OrignalItems.Dispose();
    
    public override string ToString() =>
        $"{DebugByteString.ToEscapedAscii(Hindsight)}[{DebugByteString.ToEscapedAscii([Current])}]{DebugByteString.ToEscapedAscii(Foresight)}";
}