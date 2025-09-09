using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class DebugReader(ICountingEnumerator<byte> OrignalItems) : ICountingEnumerator<byte>
{
    private readonly Queue<byte> Foresight = new Queue<byte>(20);
    private readonly BoundedQueue<byte> Hindsight = new BoundedQueue<byte>(40);
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

            while (Foresight.Count < Foresight.Capacity && OrignalItems.MoveNext())
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
            GlobalLog.Instance?.Verbose("{hindsight}[{current}]{foresight}", DebugByteString.ToEscapedAscii(Hindsight),
                DebugByteString.ToEscapedAscii([Current]), DebugByteString.ToEscapedAscii(Foresight));
        }
    }

    public void Reset()
    {
        AtEnd = false;
        Cursor = 0;
        Hindsight.Clear();
        Foresight.Clear();
        OrignalItems.Reset();
    }

    public void Dispose() => OrignalItems.Dispose();
    
    public override string ToString() =>
        $"{DebugByteString.ToEscapedAscii(Hindsight)}[{DebugByteString.ToEscapedAscii([Current])}]{DebugByteString.ToEscapedAscii(Foresight)}";
}