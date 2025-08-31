using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;
public class LazyValueGroup(UrlEncodedTokenReader reader) : IGrouping<string, object>
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
    public LazyValueGroup? NextGroup = null;
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}