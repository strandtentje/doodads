#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class CircularStack<T> : IDisposable
{
    private List<T> _items;
    private int _topIndex;

    public CircularStack()
    {
        _items = new List<T>();
        _topIndex = -1;
    }

    // Push an item onto the stack
    public void Push(T item)
    {
        // If stack is empty, start with the first item
        if (_items.Count == 0)
        {
            _items.Add(item);
            _topIndex = 0;
        }
        else
        {
            _items.Add(item);
        }
    }

    // Pop an item from the stack, but instead of removing it, we "wrap around"
    public T Pop()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Stack is empty.");
        }

        // Retrieve the current top item
        T topItem = _items[_topIndex];

        // Move the top index to the previous item, wrapping around to the beginning
        _topIndex = (_topIndex + 1) % _items.Count;

        return topItem;
    }

    // Peek at the top item without removing it
    public T Peek()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Stack is empty.");
        }

        return _items[_topIndex];
    }

    public void Dispose()
    {
        _items.Clear();
    }

    // Get the current count of items in the stack
    public int Count => _items.Count;
}
