#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
#pragma warning disable 67

[Category("Printing & Formatting")]
[Title("Cache by keys, invalidate by keys")]
[Description("Use when UseSinkCache had a cache miss and requires new data to be determined.")]
public class WriteCache : IService
{
    [PrimarySetting("""
                    Mime-type to default to (defaults to application/octet-stream) but will generally honour the 
                    content type that was previously set.
                    """)]
    private readonly UpdatingPrimaryValue DefaultMimeTypeConstant = new();
    private string CurrentDefaultMimeType = "application/octet-stream";
    [EventOccasion("Sink stream to cache here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("""
                   Either happens when there was no original sinking interaction to mimic, or when we attempted to
                   cache without cache access.
                   """)]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, DefaultMimeTypeConstant).IsRereadRequired(out string? candidateMimeType))
            this.CurrentDefaultMimeType = candidateMimeType ?? "application/octet-stream";

        if (!interaction.TryGetClosest(out ISinkingInteraction? parentInteraction) ||
            parentInteraction is null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Sinking interaction required to mimic"));
            return;
        }

        if (!interaction.TryGetClosest(out CacheMissInteraction? parentCacheMiss) ||
            parentCacheMiss is null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Can only use this for filling in cache misses"));
            return;
        }

        var bsi = BinarySinkingInteraction.CreateIntermediateFor(parentInteraction, interaction);
        OnThen?.Invoke(this, bsi);
        parentCacheMiss.Value.SetDataAndValues(bsi.SinkTrueContentType ?? this.CurrentDefaultMimeType,
            parentCacheMiss.ValidateKeys, bsi.GetData());
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}