namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public class SingleValueEnumerator<T>(T value) : IEnumerator<T>
{
    private bool HasMoved = false;
    public T Current => value;
    object IEnumerator.Current => Current ?? throw new InvalidOperationException("No value; MoveNext first");
    public bool MoveNext()
    {
        if (HasMoved) return false;
        HasMoved = true;
        return true;
    }

    public void Reset() => throw new NotSupportedException();
    public void Dispose() { }
}