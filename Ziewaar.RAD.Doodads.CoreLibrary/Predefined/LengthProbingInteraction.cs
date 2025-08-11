#nullable enable
using System.Threading;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined
{
#pragma warning disable 67
    public class LengthProbingInteraction(IInteraction parent) : IProbeContentLengthInteraction
    {
        public IInteraction Stack => parent;
        public object Register => parent.Register;
        public IReadOnlyDictionary<string, object> Memory => parent.Memory;
        private long Accumulator = -1;
        public void AddContentLength(long contentLength) => Interlocked.Add(ref this.Accumulator, contentLength);
        public void SetContentLength(long total) => Interlocked.Exchange(ref this.Accumulator, total);

        public bool TryGetContentLength(out long contentLength)
        {
            contentLength = Interlocked.Read(ref this.Accumulator);
            return contentLength > -1;
        }
    }
}