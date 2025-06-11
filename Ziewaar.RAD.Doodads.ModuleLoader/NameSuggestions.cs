#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class NameSuggestions(IEnumerable<string> sourceList)
{
    private List<(string Original, SortedSet<string> Trigrams)>? _entries;
    private readonly SortedList<string, SortedSet<string>> _trigramsCachedByName = new();
    private readonly SortedList<string, string> _cachedSuggestions = new();
    public string GetMostSimilar(string input, SortedSet<string>? inputTrigrams = null) =>
        inputTrigrams == null && _cachedSuggestions.TryGetValue(input, out string suggestion) ?
        suggestion :
        _cachedSuggestions[input] = GetEntries()
            .Select(entry => new
            {
                entry.Original,
                Score = JaccardSimilarity(inputTrigrams ??= GetTrigrams(input), entry.Trigrams)
            })
            .OrderByDescending(x => x.Score).First().Original;
    private List<(string Original, SortedSet<string> Trigrams)> GetEntries() => 
        _entries ??= sourceList.Select(s => (Original: s, Trigrams: GetTrigrams(s))).ToList();
    private SortedSet<string> GetTrigrams(string str)
    {
        if (_trigramsCachedByName.TryGetValue(str, out var set))
            return set;

        str = str.ToLowerInvariant().Replace(" ", "");
        var trigrams = new SortedSet<string>();
        for (int i = 0; i < str.Length - 2; i++)
            trigrams.Add(str.Substring(i, 3));
        _trigramsCachedByName[str] = trigrams;
        return trigrams;
    }
    private static double JaccardSimilarity(SortedSet<string> a, SortedSet<string> b)
    {
        var intersection = a.Intersect(b).Count();
        var union = a.Union(b).Count();
        return union == 0 ? 0 : (double)intersection / union;
    }
}