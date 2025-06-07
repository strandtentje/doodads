using System.IO;
using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio
{
    public class ConsoleToVariableInteraction(IInteraction interaction, string key, string value) : IInteraction
    {
        public IInteraction Parent => interaction;
        public IReadOnlyDictionary<string, object> Variables { get; } = new SortedList<string, object>()
        {
            { key, value }
        };
    }
    public class ConsoleSourceInteraction : ISourcingInteraction<Stream>
    {
        public ConsoleSourceInteraction(IInteraction parent, Stream stream)
        {
            this.Parent = parent;
            this.Variables = this.Parent.Variables;
            this.TaggedData = new StdioStreamData(stream);
        }
        public ITaggedData<Stream> TaggedData { get; }
        public IInteraction Parent { get; }
        public IReadOnlyDictionary<string, object> Variables { get; }
    }
}
