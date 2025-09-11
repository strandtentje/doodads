#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class BinarySinkingInteraction(IInteraction parent) : ISinkingInteraction, IDisposable
{
    private bool disposedValue;
    public Encoding TextEncoding { get; private set;} = NoEncoding.Instance;
    private readonly MemoryStream buffer = new MemoryStream();
    public Stream SinkBuffer => buffer;
    public string[] SinkContentTypePattern { get; private set;  } = ["*/*"];
    public string? SinkTrueContentType { get; set; }
    public long LastSinkChangeTimestamp { get; set; }
    public string Delimiter { get; private set;} = "";
    public IInteraction Stack { get; } = parent;
    public object Register { get; private set;} = parent.Register;
    public IReadOnlyDictionary<string, object> Memory { get; private set; } = parent.Memory;
    public static BinarySinkingInteraction CreateIntermediateFor(ISinkingInteraction original, IInteraction offset,
        object? register = null, IReadOnlyDictionary<string, object>? memory = null) =>
        new(offset)
        {
            SinkContentTypePattern = original.SinkContentTypePattern,
            SinkTrueContentType = original.SinkTrueContentType,
            Delimiter = original.Delimiter,
            TextEncoding = original.TextEncoding,
            Register = register ?? offset.Register,
            Memory = memory ?? offset.Memory
        };
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

    public byte[] GetData() => buffer.ToArray();
}
