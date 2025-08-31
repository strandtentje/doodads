using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;
public class LazyFormDataDictionary(UrlEncodedTokenReader reader) :
    IEnumerable<LazyValueGroup>
{
    public IEnumerator<LazyValueGroup> GetEnumerator()
    {
        SortedSet<string> seenKeys = new();
        if (reader.MoveNext())
        {
            LazyValueGroup? valueGroup = new LazyValueGroup(reader);
            do
            {
                if (!seenKeys.Add(valueGroup.Key))
                    throw new ConsecutiveKeyException("Form keys may only be consecutive. Design forms accordingly.");
                yield return valueGroup;
                valueGroup = valueGroup.NextGroup;
            } while (valueGroup != null);
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}