#nullable enable
using System.Text;
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

public class TextSourcingInteraction(IInteraction parent, Stream data, Encoding encoding, MimeMapping.MimeTypeInfo mime) : ISourcingInteraction ){
    public Stream SourceBuffer => data;
    public Encoding TextEncoding => encoding;
    public string SourceContentTypePattern => mime.MimeType;
    public long SourceContentLength => data.Length;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
}
