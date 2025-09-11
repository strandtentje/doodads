using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;


namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public class StreamingFormDataEnumerable(UrlEncodedTokenReader reader) :
    IEnumerable<StreamingFormDataValueGroup>
{
    public IEnumerator<StreamingFormDataValueGroup> GetEnumerator()
    {
        SortedSet<string> seenKeys = new();
        if (reader.MoveNext())
        {
            StreamingFormDataValueGroup? valueGroup = new StreamingFormDataValueGroup(reader);
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