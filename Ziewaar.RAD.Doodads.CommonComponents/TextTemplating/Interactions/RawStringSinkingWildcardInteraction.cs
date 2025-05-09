namespace Ziewaar.RAD.Doodads.CommonComponents;

public class RawStringSinkingWildcardInteraction : IWildcardTargetInteraction, ISinkingInteraction<StreamWriter>
{
    private readonly MemoryStream Buffer;

    public RawStringSinkingWildcardInteraction(IInteraction parent, string pattern)
    {
        this.Parent = parent;
        this.Pattern = pattern;
        this.Buffer = new MemoryStream();
        this.TaggedData = new AlwaysInDemandTaggedStringData(new StreamWriter(this.Buffer));
    }

    public IInteraction Parent { get; }
    public string Pattern { get; }
    public virtual SortedList<string, object> Variables => Parent.Variables;
    public ITaggedData<StreamWriter> TaggedData { get; }

    public string GetFullString()
    {
        using var reader = new StreamReader(Buffer);
        return reader.ReadToEnd();
    }
}