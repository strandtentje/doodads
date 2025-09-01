namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public static class PrefixSearchIndex
{
    public static int[] Build(ReadOnlySpan<byte> pattern)
    {
        int[] Table = new int[pattern.Length];
        int Length = 0;
        
        for (int i = 1; i < pattern.Length; i++)
        {
            while (Length > 0 && pattern[i] != pattern[Length])
                Length = Table[Length - 1];


            if (pattern[i] == pattern[Length])
                Length++;


            Table[i] = Length;
        }


        return Table;
    }
}