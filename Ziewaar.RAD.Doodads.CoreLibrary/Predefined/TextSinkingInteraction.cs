#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
public class TextSinkingInteraction(
    IInteraction parent,
    string[]? pattern = null,
    string delimiter = "",
    Encoding? textEncoding = null,
    object? register = null,
    IReadOnlyDictionary<string, object>? memory = null)
    : ISinkingInteraction
{
    public IInteraction Stack { get; } = parent;
    public object Register { get; } = register ?? parent.Register;
    public IReadOnlyDictionary<string, object> Memory { get; } = memory ?? parent.Memory;
    public Encoding TextEncoding { get; } = textEncoding ?? Encoding.Unicode;
    public Stream SinkBuffer { get; } = new MemoryStream();
    public string[] SinkContentTypePattern { get; } = pattern ?? ["*/*"];
    public string? SinkTrueContentType { get; set; }
    public long LastSinkChangeTimestamp { get; set; }
    public string Delimiter { get; } = delimiter;
    public static TextSinkingInteraction CreateIntermediateFor(ISinkingInteraction original, IInteraction offset,
        object? register = null, IReadOnlyDictionary<string, object>? memory = null) =>
        new(offset, original.SinkContentTypePattern, original.Delimiter,
            original.TextEncoding, register, memory);
    public void SetContentLength64(long contentLength)
    {
    }
}
