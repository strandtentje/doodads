using System.Text;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public static class MultipartBoundaryFinder
{
    public static bool SkipToFirstBoundary(this Stream stream, byte[] boundary) => StreamPatternFinder.Seek(stream, AsBoundaryPrefix("--", boundary));
    public static bool ExpectNextBoundary(this Stream stream, byte[] boundary) => StreamPatternFinder.Seek(stream, AsBoundaryPrefix("\r\n--", boundary));
    private static byte[] AsBoundaryPrefix(string prefix, byte[] boundary) => [..Encoding.ASCII.GetBytes(prefix), ..boundary];
}