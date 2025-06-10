using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio
{
    public class ConsoleSinkInteraction(IInteraction parent, Stream stream) : ISinkingInteraction
    {
        public IInteraction Stack => parent;
        public object Register => parent;
        public IReadOnlyDictionary<string, object> Memory => parent.Memory;
        public Encoding TextEncoding => Encoding.Default;
        public Stream SinkBuffer => stream;
        public string[] SinkContentTypePattern { get; } = ["text/*"];
        public string SinkTrueContentType { get; set; }
        public long LastSinkChangeTimestamp { get; set; }
        public string Delimiter => "\n";
        public void SetContentLength64(long contentLength)
        {
        }
    }
}
