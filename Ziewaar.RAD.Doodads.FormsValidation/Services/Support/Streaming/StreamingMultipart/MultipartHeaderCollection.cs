using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;
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
                asciiHeader.Split(':', 2, (StringSplitOptions)3) is not string[] keyValueArr ||
                keyValueArr.ElementAtOrDefault(0) is not string keyCandidate ||
                string.IsNullOrWhiteSpace(keyCandidate) ||
                keyValueArr.ElementAtOrDefault(1) is not string valueCandidate ||
                string.IsNullOrWhiteSpace(valueCandidate))
                yield break;
            else
                yield return new MultipartHeader(keyCandidate, valueCandidate.GetBaseHeader(""),
                    valueCandidate.GetHeaderProperties());
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}