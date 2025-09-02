using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;
public class UntilBoundaryReader(MultibyteEotReader signalReader, MultibyteEotReader boundaryReader)
    : ICountingEnumerator<byte>
{
    public byte Current { get; private set; }
    private readonly Queue<byte> ByteQueue =
        new(signalReader.DetectionBuffer.Length + boundaryReader.DetectionBuffer.Length);
    object? IEnumerator.Current => Current;
    public bool AtEnd { get; private set; }
    public long Cursor { get; private set; }
    public string? ErrorState { get; set; }
    public bool MoveNext()
    {
        if (ByteQueue.TryDequeue(out var dud))
        {
            Current = dud;
            Cursor++;
            return true;
        }
        else if (signalReader.MoveNext())
        {
            this.Current = signalReader.Current;
            Cursor++;
            return true;
        }
        else if (boundaryReader.MoveNext())
        {
            signalReader.ForSelection(ByteQueue.Enqueue);
            signalReader.Reset();
            ByteQueue.Enqueue(boundaryReader.Current);
            Current = ByteQueue.Dequeue();
            Cursor++;
            return true;
        }
        else
        {
            AtEnd = true;
            return false;
        }
    }
    public void Reset()
    {
        ByteQueue.Clear();
        AtEnd = false;
        Cursor = 0;
        ErrorState = null;
        signalReader.Reset();
        boundaryReader.Reset();
    }
    public void Dispose()
    {
    }
}