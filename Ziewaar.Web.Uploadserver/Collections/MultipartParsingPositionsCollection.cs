namespace Ziewaar.Web.Uploadserver.Collections;

public class MultipartParsingPositionsCollection : IEnumerable<long>
{
    public long
        BoundaryStart, BoundaryEnd,
        HeaderStart, HeaderEnd,
        ContentStart, Break, ContentEnd;

    public long ConsumedUntil
    {
        get
        {
            return this.Max();
        }
    }

    public IEnumerator<long> GetEnumerator()
    {
        yield return BoundaryStart;
        yield return BoundaryEnd;
        yield return HeaderStart;
        yield return HeaderEnd;
        yield return ContentStart;
        yield return Break;
        yield return ContentEnd;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}