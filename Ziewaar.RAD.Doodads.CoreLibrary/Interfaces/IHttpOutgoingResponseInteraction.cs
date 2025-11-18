#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface IHttpOutgoingResponseInteraction : ISinkingInteraction
{
    int StatusCode { get; set; }
    void RedirectTo(string url, bool preservePost = false);
}