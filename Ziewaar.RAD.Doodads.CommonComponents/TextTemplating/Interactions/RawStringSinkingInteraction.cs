namespace Ziewaar.RAD.Doodads.CommonComponents;

public class RawStringSinkingInteraction : ISinkingInteraction<StreamWriter>
{
    private readonly MemoryStream Buffer;
    public RawStringSinkingInteraction(IInteraction parent, string delimiter = "", SidechannelState suggestedState = SidechannelState.Always)
    {
        this.Parent = parent;
        this.Buffer = new MemoryStream();
        this.TaggedData = new TaggedStreamWriterData(new StreamWriter(this.Buffer), suggestedState);
        this.Delimiter = delimiter;
    }
    public IInteraction Parent { get; }
    public virtual IReadOnlyDictionary<string, object> Variables => Parent.Variables;
    public ITaggedData<StreamWriter> TaggedData { get; }
    public string Delimiter { get; }
    public string GetFullString()
    {
        using var reader = new StreamReader(Buffer);
        return reader.ReadToEnd();
    }
}
