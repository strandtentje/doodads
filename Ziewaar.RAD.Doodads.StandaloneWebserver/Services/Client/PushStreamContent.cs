using System.Net.Http.Headers;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Client;
#pragma warning disable 67
public class PushStreamContent(
    StreamAvailabilityHandler dataHandler)
    : HttpContent
{
    protected override void SerializeToStream(Stream stream, TransportContext? context,
        CancellationToken cancellationToken) =>
        dataHandler(stream, context);

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
        Task.Run(() => SerializeToStream(stream, context, CancellationToken.None));

    protected override bool TryComputeLength(out long length)
    {
        length = -1;
        return false;
    }
}