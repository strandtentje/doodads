namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface ISourcingInteraction : IInteraction
{
    Stream SourceBuffer { get; }
    Encoding TextEncoding { get; }
    string SourceContentTypePattern { get; }
    long SourceContentLength { get; }
}