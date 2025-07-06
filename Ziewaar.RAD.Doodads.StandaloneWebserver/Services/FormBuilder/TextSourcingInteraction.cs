namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public class TextSourcingInteraction : ISourcingInteraction, IDisposable, IAsyncDisposable
{
    private readonly IInteraction Parent;
    public TextSourcingInteraction(IInteraction parent, string text)
    {
        Parent = parent;
        var bytes = Encoding.UTF8.GetBytes(text);
        SourceBuffer = new MemoryStream(bytes);
        SourceContentLength = bytes.Length;
    }
    public IInteraction Stack => Parent;
    public object Register => Parent.Register;
    public IReadOnlyDictionary<string, object> Memory => Parent.Memory;
    public Stream SourceBuffer { get; }
    public Encoding TextEncoding { get; } = Encoding.UTF8;
    public string SourceContentTypePattern { get; } = "*/*";
    public long SourceContentLength { get; } 
    public void Dispose() => SourceBuffer.Dispose();
    public async ValueTask DisposeAsync() => await SourceBuffer.DisposeAsync();
}