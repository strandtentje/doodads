namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio
{
    internal class StdioStreamData(Stream stream) : ITaggedData<Stream>
    {
        public Stream Data => stream;
        public SidechannelTag Tag { get; set; } = SidechannelTag.Always;
    }
}
