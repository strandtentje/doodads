using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingMultipart;
public interface ITaggedCountingEnumerator<T> : ICountingEnumerator<T>
{
    object Tag { get; set; }
}