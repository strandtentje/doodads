namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class BoundedQueue<T> : IEnumerable<T>
{
    private readonly Queue<T> Queue;

    /// <summary>
    /// Maximum number of items the queue will hold.
    /// Oldest items will be discarded when exceeded.
    /// </summary>
    public int Capacity { get; }

    public BoundedQueue(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");

        Capacity = capacity;
        Queue = new Queue<T>(capacity);
    }

    /// <summary>
    /// Adds an item to the queue. If capacity is exceeded, removes the oldest item.
    /// </summary>
    public void Enqueue(T item)
    {
        if (Queue.Count >= Capacity)
            Queue.Dequeue(); // Evict oldest
        Queue.Enqueue(item);
    }

    /// <summary>
    /// Removes and returns the oldest item in the queue.
    /// </summary>
    public T Dequeue() => Queue.Dequeue();

    /// <summary>
    /// Returns the number of items currently in the queue.
    /// </summary>
    public int Count => Queue.Count;

    /// <summary>
    /// Clears all items from the queue.
    /// </summary>
    public void Clear() => Queue.Clear();

    /// <summary>
    /// Returns an enumerator over the current queue contents (in order).
    /// </summary>
    public IEnumerator<T> GetEnumerator() => Queue.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}