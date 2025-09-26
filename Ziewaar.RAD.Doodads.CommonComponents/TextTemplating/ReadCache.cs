#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
#pragma warning disable 67

[Category("Printing & Formatting")]
[Title("Cache by keys, invalidate by keys")]
[Description("Use when UseSinkCache had a cache hit and didn't detect invalid validation values.")]
public class ReadCache : IService
{
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest(out ISinkingInteraction? parentInteraction) ||
            parentInteraction is null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Sinking interaction required to mimic"));
            return;
        }

        if (!interaction.TryGetClosest(out CacheHitInteraction? parentCacheHit) ||
            parentCacheHit is null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Can only use this for fulfilling cache hits."));
            return;
        }

        parentCacheHit.Value.CopyTo(parentInteraction.SinkBuffer, out string mimeType);
        parentInteraction.SinkTrueContentType = mimeType;
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}