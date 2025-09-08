#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

[Category("Printing & Formatting")]
[Title("Cache by keys, invalidate by keys")]
[Description("Use when UseSinkCache had a cache miss and requires new data to be determined.")]
public class WriteCache : IService
{
    private readonly UpdatingPrimaryValue DefaultMimeTypeConstant = new();
    private string CurrentDefaultMimeType = "application/octet-stream";
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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