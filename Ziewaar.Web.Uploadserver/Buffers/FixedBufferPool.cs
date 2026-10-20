namespace Ziewaar.Web.Uploadserver.Buffers;

public class FixedBufferPool : IDisposable
{
    public readonly int Capacity;
    public bool IsDisposing { get; private set; }
        
    private readonly Queue<FixedBuffer> BackingBuffers;
    private readonly Semaphore UnderrunGuard, OverrunGuard;
    private readonly object QueueLock = new object();

    public FixedBufferPool(int capacity, params byte[][] allocatedBuffers)
    {
        this.Capacity = capacity;
        this.UnderrunGuard = new Semaphore(0, capacity);
        this.OverrunGuard = new Semaphore(capacity, capacity);
        this.BackingBuffers = new Queue<FixedBuffer>();

        foreach (var allocation in allocatedBuffers)
        {
            BackingBuffers.Enqueue(new FixedBuffer(
                allocation, allocation.Length));
            this.UnderrunGuard.Release(1);
            if (!this.OverrunGuard.WaitOne(100))
                throw new ArgumentException(
                    "Allocated buffer count may not exceed capacity");
        }
    }

    public static FixedBufferPool CreatePreAllocated(
        int capacity, int bufferSize)
    {
        var AllocatedArrays = new List<byte[]>();
        for (int bufferIndex = 0; bufferIndex < capacity; bufferIndex++)
        {
            AllocatedArrays.Add(new byte[bufferSize]);
        }
        return new FixedBufferPool(capacity, AllocatedArrays.ToArray());
    }

    public int Enqueue(byte[] buffer, int count)
    {
        OverrunGuard.WaitOne();
        lock (QueueLock)
        {
            BackingBuffers.Enqueue(new FixedBuffer(buffer, count));
            count = BackingBuffers.Count;
        }
        UnderrunGuard.Release(1);
        return count;
    }

    public bool TryDequeue(out byte[] buffer, out int count)
    {
        if (UnderrunGuard.WaitOne(1000))
        {
            lock (QueueLock)
            {
                var result = BackingBuffers.Dequeue();
                buffer = result.Allocation;
                count = result.Count;
            }
            OverrunGuard.Release(1);
            return true;
        }
        else
        {
            buffer = FixedBuffer.Null.Allocation;
            count = FixedBuffer.Null.Count;
            return false;
        }
    }

    public void Dispose()
    {
        this.IsDisposing = true;
        UnderrunGuard.Close();
        OverrunGuard.Close();
        BackingBuffers.Clear();
    }
}