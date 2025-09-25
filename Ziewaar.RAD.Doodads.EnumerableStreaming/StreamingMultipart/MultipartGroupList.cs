using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingMultipart;
public class MultipartGroupList(ICountingEnumerator<byte> byteReader, string boundaryAscii) :
    IEnumerable<IGrouping<string, object>>
{
    private readonly MultibyteEotReader boundaryDetector = MultibyteEotReader.CreateForAscii(byteReader, boundaryAscii);
    private readonly CrlfDetector crlfDetector = new CrlfDetector(byteReader);
    public IEnumerator<IGrouping<string, object>> GetEnumerator()
    {
        ConsumeMultipartPreamble();
        MultipartValueGroup? group = new MultipartValueGroup(crlfDetector, boundaryDetector);
        while (!string.IsNullOrWhiteSpace(group?.Key))
        {
            yield return group;
            group = group.NextGroup;
        }
    }
    private void ConsumeMultipartPreamble()
    {
        while (boundaryDetector.MoveNext()) ;
        boundaryDetector.Reset();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}