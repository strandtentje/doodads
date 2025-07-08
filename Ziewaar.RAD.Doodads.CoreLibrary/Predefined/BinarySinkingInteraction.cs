#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class BinarySinkingInteraction(IInteraction parent) : ISinkingInteraction, IDisposable
{
    private bool disposedValue;
    public Encoding TextEncoding { get; } = NoEncoding.Instance;
    public Stream SinkBuffer { get; } = new MemoryStream();
    public string[] SinkContentTypePattern { get; } = ["*/*"];
    public string? SinkTrueContentType { get; set; }
    public long LastSinkChangeTimestamp { get; set; }
    public string Delimiter { get; } = "";
    public IInteraction Stack { get; } = parent;
    public object Register { get; } = parent.Register;
    public IReadOnlyDictionary<string, object> Memory { get; } = parent.Memory;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                SinkBuffer.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~BinarySinkingInteraction()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void SetContentLength64(long contentLength)
    {

    }
}
