namespace Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;
public interface ICountingEnumerator<TType> : IEnumerator<TType>
{
    public bool AtEnd { get; }
    public long Cursor { get; }
    public string? ErrorState { get; set; }
}