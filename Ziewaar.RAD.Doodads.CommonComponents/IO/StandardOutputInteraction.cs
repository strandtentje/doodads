using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.IO;

public class StandardOutputInteraction(IInteraction parent, Stream outputStream, Encoding encoding = null) : ISourcingInteraction
{
    public Stream SourceBuffer => outputStream;
    public Encoding TextEncoding => encoding ?? Console.OutputEncoding ?? Encoding.Default;
    public string SourceContentTypePattern => "application/octet-stream";
    public long SourceContentLength => -1;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
}