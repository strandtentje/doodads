namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public class StreamingMultipartDataValueGroup(MultipartHeader header, Stream body) : IGrouping<string, object>
{
    public string Key => header.Name;

    public IEnumerator<object> GetEnumerator() =>
        header.FileName != null
            ? new SingleValueEnumerator<object>(new StreamByteEnumerator(body))
            : new SingleValueEnumerator<object>(new StreamCharEnumerator(body, header.Headers));

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}