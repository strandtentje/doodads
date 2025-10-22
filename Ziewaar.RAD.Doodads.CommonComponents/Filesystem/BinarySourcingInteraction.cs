#nullable enable
using System.Text;
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

public class BinarySourcingInteraction(IInteraction parent, Stream data, MimeMapping.MimeTypeInfo mime) : ISourcingInteraction
{
    public Stream SourceBuffer => data;
    public Encoding TextEncoding => NoEncoding.Instance;
    public string SourceContentTypePattern => mime.MimeType;
    public long SourceContentLength => data.Length;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
}
