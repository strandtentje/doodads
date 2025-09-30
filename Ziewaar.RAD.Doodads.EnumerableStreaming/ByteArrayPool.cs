using System.Collections.Concurrent;
using System.Threading;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support;

public sealed class ByteArrayPool
{
    private readonly ConcurrentQueue<byte[]> _queue = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly int _arraySize;
    private readonly TimeSpan _timeout;

    public int ArraySize => _arraySize;
    public int Capacity { get; }

    public ByteArrayPool(int arrayCount, int arraySize, TimeSpan timeout)
    {
        if (arrayCount <= 0) throw new ArgumentOutOfRangeException(nameof(arrayCount));
        if (arraySize <= 0) throw new ArgumentOutOfRangeException(nameof(arraySize));

        Capacity = arrayCount;
        _arraySize = arraySize;
        _timeout = timeout;
        _semaphore = new SemaphoreSlim(arrayCount, arrayCount);

        for (int i = 0; i < arrayCount; i++)
        {
            _queue.Enqueue(new byte[arraySize]);
        }
    }

    public byte[] Rent(CancellationToken cancellationToken = default)
    {
        if (!_semaphore.Wait(_timeout))
            throw new TimeoutException("Timeout while waiting to rent a byte array.");

        if (_queue.TryDequeue(out var buffer))
            return buffer;

        // This should rarely happen due to semaphore enforcement.
        _semaphore.Release();
        throw new InvalidOperationException("Inconsistent pool state.");
    }

    public void Return(byte[] buffer)
    {
        if (buffer == null || buffer.Length != _arraySize)
            throw new ArgumentException("Returned array does not match pool specification.");

        Array.Clear(buffer, 0, buffer.Length);
        
        _queue.Enqueue(buffer);
        _semaphore.Release();
    }
}