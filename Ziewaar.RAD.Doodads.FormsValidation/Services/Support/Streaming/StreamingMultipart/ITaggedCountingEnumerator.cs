using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;
public interface ITaggedCountingEnumerator<T> : ICountingEnumerator<T>
{
    object Tag { get; set; }
}