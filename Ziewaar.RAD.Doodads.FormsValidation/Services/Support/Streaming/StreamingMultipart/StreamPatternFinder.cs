namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public static class StreamPatternFinder
{
    public static bool Seek(Stream stream, byte[] pattern)
    {
        var matcher = new BoundaryMatcher(pattern);
        MatchResult result = MatchResult.NoMatch;
        for (int next = stream.ReadByte();
             next != -1 && !matcher.TryCompleteMatch((byte)next, out result);
             next = stream.ReadByte()) ;
        return result == MatchResult.MatchComplete;
    }
}