using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingMultipart;

public class MultipartHeaderCollection(ICountingEnumerator<byte> crlfDetector) : IEnumerable<MultipartHeader>
{
    public IEnumerator<MultipartHeader> GetEnumerator()
    {
        int count = 0;
        while (true)
        {
            var asciiHeader = crlfDetector.ToAscii();
            crlfDetector.Reset();
            if (count++ == 0 && asciiHeader.Length == 0)
            {
                asciiHeader = crlfDetector.ToAscii();
                crlfDetector.Reset();
            }

            if (string.IsNullOrWhiteSpace(asciiHeader) ||
                asciiHeader.Split([':'], 2, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() is not { } keyValueArr ||
                keyValueArr.ElementAtOrDefault(0) is not { } keyCandidate ||
                string.IsNullOrWhiteSpace(keyCandidate) ||
                keyValueArr.ElementAtOrDefault(1) is not { } valueCandidate ||
                string.IsNullOrWhiteSpace(valueCandidate))
                yield break;
            else
                yield return new MultipartHeader(keyCandidate, valueCandidate.GetBaseHeader(""),
                    valueCandidate.GetHeaderProperties());
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}