namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio
{
    public class ConsoleSinkInteraction(IInteraction parent, Stream stream) : ISinkingInteraction<Stream>
    {
        public ITaggedData<Stream> TaggedData { get; } = new StdioStreamData(stream);
        public string Delimiter => "\n";
        public IInteraction Parent => parent;
        public IReadOnlyDictionary<string, object> Variables => parent.Variables;
    }
}
