namespace Ziewaar.RAD.Doodads.CommonComponents;

public class RawStringAlwaysSinkingInteraction : ISinkingInteraction<StreamWriter>
{
    private readonly MemoryStream Buffer;

    public RawStringAlwaysSinkingInteraction(IInteraction parent, SidechannelState suggestedState = SidechannelState.Always)
    {
        this.Parent = parent;
        this.Buffer = new MemoryStream();
        this.TaggedData = new TaggedStreamWriterData(new StreamWriter(this.Buffer), suggestedState);
    }

    public IInteraction Parent { get; }
    public virtual SortedList<string, object> Variables => Parent.Variables;
    public ITaggedData<StreamWriter> TaggedData { get; }

    public string GetFullString()
    {
        using var reader = new StreamReader(Buffer);
        return reader.ReadToEnd();
    }
}
