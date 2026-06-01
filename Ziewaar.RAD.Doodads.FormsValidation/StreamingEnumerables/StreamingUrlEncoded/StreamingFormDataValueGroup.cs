using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingUrlEncoded;
public class StreamingFormDataValueGroup(UrlEncodedTokenReader reader) : IGrouping<string, object>
{
    public string Key { get; } = reader.Current.Key;
    public IEnumerator<object> GetEnumerator()
    {
        bool wasMoved;
        bool myKey;
        do
        {
            yield return reader.Current.Value;
            wasMoved = reader.MoveNext();
            myKey = reader.Current.Key == this.Key;
        } while (wasMoved && myKey);
        if (wasMoved && !myKey)
            NextGroup = new(reader);
        else
            NextGroup = null;
    }
    public StreamingFormDataValueGroup? NextGroup = null;
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}