using System.Text;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
#nullable enable
public interface ISinkingInteraction : IInteraction
{
    Encoding TextEncoding { get; }
    Stream SinkBuffer { get; }
    string[] SinkContentTypePattern { get; }
    string? SinkTrueContentType { get; set; }
    long LastSinkChangeTimestamp { get; set; }
    string Delimiter { get; }
}