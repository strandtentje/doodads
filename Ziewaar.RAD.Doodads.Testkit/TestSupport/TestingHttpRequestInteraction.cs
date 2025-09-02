using System.Net;
using System.Text;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Testkit;
public class TestingHttpRequestInteraction(
    IInteraction parent,
    CookieCollection incomingCookies,
    string mimeType,
    long length,
    Stream body
) :
    ISourcingInteraction, IContentTypePropertiesInteraction
{
    public CookieCollection IncomingCookies => incomingCookies;
    public Stream SourceBuffer => body;
    public Encoding TextEncoding => Encoding.UTF8;
    public string SourceContentTypePattern => mimeType;
    public long SourceContentLength => length;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public IReadOnlyDictionary<string, string> ContentTypeProperties => mimeType.GetHeaderProperties();
}