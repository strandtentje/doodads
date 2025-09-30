#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods
{
    public class ResurfacedSinkingInteraction(
        IInteraction canonicalInteraction,
        ISinkingInteraction sinkingInteraction)
        : ISinkingInteraction
    {
        public IInteraction Stack => canonicalInteraction;
        public object Register => canonicalInteraction.Register;

        public IReadOnlyDictionary<string, object> Memory =>
            canonicalInteraction.Memory;

        public Encoding TextEncoding => sinkingInteraction.TextEncoding;
        public Stream SinkBuffer => sinkingInteraction.SinkBuffer;

        public string[] SinkContentTypePattern =>
            sinkingInteraction.SinkContentTypePattern;

        public string? SinkTrueContentType
        {
            get => sinkingInteraction.SinkTrueContentType;
            set => sinkingInteraction.SinkTrueContentType = value;
        }

        public long LastSinkChangeTimestamp
        {
            get => sinkingInteraction.LastSinkChangeTimestamp;
            set => sinkingInteraction.LastSinkChangeTimestamp = value;
        }

        public string Delimiter => sinkingInteraction.Delimiter;

        public void SetContentLength64(long contentLength) =>
            sinkingInteraction.SetContentLength64(contentLength);
    }
}