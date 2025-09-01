namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public class BoundaryMatcher(byte[] pattern)
{
    private readonly int[] PrefixTable = PrefixSearchIndex.Build(pattern);
    private int MatchIndex = 0;

    public MatchResult AdvanceMatch(byte nextByte)
    {
        TryCompleteMatch(nextByte, out var result);
        return result;
    }
    public bool TryCompleteMatch(byte nextByte, out MatchResult result)
    {
        result = MatchResult.NoMatch;
        AdjustMatchIndexOnMismatch(nextByte);
        if (IsCurrentByteMatching(nextByte))
            result = AdvanceMatchIndexAndReturnProgress();
        
        return result == MatchResult.MatchComplete;
    }
    private void AdjustMatchIndexOnMismatch(byte b)
    {
        while (MatchIndex > 0 && b != pattern[MatchIndex])
            MatchIndex = PrefixTable[MatchIndex - 1];
    }
    private MatchResult AdvanceMatchIndexAndReturnProgress()
    {
        MatchIndex++;
        if (!HasMatchedFullPattern())
        {
            return MatchResult.Matching;
        }
        else
        {
            MatchIndex = 0;
            return MatchResult.MatchComplete;
        }
    }
    private bool IsCurrentByteMatching(byte b) => b == pattern[MatchIndex];
    private bool HasMatchedFullPattern() => MatchIndex == pattern.Length;
}