namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Client;
#pragma warning disable 67
public class HttpClientPoolAccessor : IDisposable
{
    private readonly (Guid Guid, SocketsHttpHandler Instance) CurrentSockets;
    private readonly (Guid Guid, HttpClient Instance) CurrentClient;
    public HttpClientPoolAccessor()
    {
        this.CurrentSockets = SingletonResourceRepository<byte, SocketsHttpHandler>.Get().Take(0, HttpSocketsFactory);
        this.CurrentClient = SingletonResourceRepository<byte, HttpClient>.Get().Take(0, HttpClientFactory);
    }
    private SocketsHttpHandler HttpSocketsFactory(byte arg) => new()
    {
        AutomaticDecompression =
            DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
    };
    private HttpClient HttpClientFactory(byte arg) => new HttpClient(this.CurrentSockets.Instance, false);
    public void Dispose()
    {
        SingletonResourceRepository<byte, SocketsHttpHandler>.Get().Return(0, CurrentSockets.Guid);
        SingletonResourceRepository<byte, HttpClient>.Get().Return(0, CurrentClient.Guid);
    }
    public HttpClient Instance => CurrentClient.Instance;
}