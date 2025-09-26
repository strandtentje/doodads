using System.IO;
using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
public class ConsoleSourceInteraction(IInteraction parent, Stream inputStream) : ISourcingInteraction
{
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public Stream SourceBuffer => inputStream;
    public Encoding TextEncoding => Encoding.Default;
    public string SourceContentTypePattern { get; } = "text/*";
    public long SourceContentLength { get; } = long.MaxValue;
}