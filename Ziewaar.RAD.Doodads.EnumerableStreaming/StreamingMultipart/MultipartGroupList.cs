using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingMultipart;
public class MultipartGroupList(ICountingEnumerator<byte> byteReader, string boundaryAscii) :
    IEnumerable<IGrouping<string, object>>
{
    private readonly MultibyteEotReader BoundaryDetector = MultibyteEotReader.CreateForAscii(byteReader, boundaryAscii);
    private readonly CrlfDetector CrlfDetector = new CrlfDetector(byteReader);
    public IEnumerator<IGrouping<string, object>> GetEnumerator()
    {
        ConsumeMultipartPreamble();
        var group = new MultipartValueGroup(CrlfDetector, BoundaryDetector);
        while (group != null && !string.IsNullOrWhiteSpace(group.Key))
        {
            yield return group;
            group = group.NextGroup;
        }
    }
    private void ConsumeMultipartPreamble()
    {
        while (BoundaryDetector.MoveNext()) ;
        BoundaryDetector.Reset();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}