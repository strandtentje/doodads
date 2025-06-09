namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;

public class PrefixSinkingInteraction : ISinkingInteraction<Stream>
{
    private readonly MemoryStream PrefixData;
    public PrefixSinkingInteraction(IInteraction parent, long lastKnownStamp, SidechannelState when)
    {
        Parent = parent;
        PrefixData = new();
        TaggedData = new PrefixSinkData(PrefixData, lastKnownStamp, when);
    }
    public ITaggedData<Stream> TaggedData { get; }
    public string Delimiter => "\n";
    public IInteraction Parent { get; }
    public IReadOnlyDictionary<string, object> Variables => Parent.Variables;
    public bool HasData => PrefixData.Length > 0;
    public string[]? GetAllPrefixes(ref long after)
    {
        if (!TaggedData.Tag.IsTainted || !HasData || after >= TaggedData.Tag.Stamp)
            return null;
        after = TaggedData.Tag.Stamp;
        List<string> prefixes = [];
        using var reader = new StreamReader(PrefixData);
        while (!reader.EndOfStream)
            if (reader.ReadLine() is string prefixLine && !string.IsNullOrWhiteSpace(prefixLine))
                prefixes.Add(prefixLine);
        if (!prefixes.Any()) return null;
        return [.. prefixes];
    }
}
