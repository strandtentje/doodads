namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;

public class PrefixSinkingInteraction : ISinkingInteraction<Stream>
{
    private readonly MemoryStream prefixData;
    public PrefixSinkingInteraction(IInteraction parent, long lastKnownStamp, SidechannelState when)
    {
        Parent = parent;
        prefixData = new();
        TaggedData = new PrefixSinkData(prefixData, lastKnownStamp, when);
    }
    public ITaggedData<Stream> TaggedData { get; }
    public string Delimiter => "\n";
    public IInteraction Parent { get; }
    public IReadOnlyDictionary<string, object> Variables => Parent.Variables;
    public bool HasData => prefixData.Length > 0;
    public string[]? GetAllPrefixes(ref long after)
    {
        if (!TaggedData.Tag.IsTainted || !HasData || after >= TaggedData.Tag.Stamp)
            return null;
        after = TaggedData.Tag.Stamp;
        List<string> prefixes = [];
        using var reader = new StreamReader(prefixData);
        while (!reader.EndOfStream)
            if (reader.ReadLine() is string prefixLine && !string.IsNullOrWhiteSpace(prefixLine))
                prefixes.Add(prefixLine);
        if (!prefixes.Any()) return null;
        return [.. prefixes];
    }
}
