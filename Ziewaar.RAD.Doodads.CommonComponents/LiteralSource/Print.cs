#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

public class Print : IService
{
    private readonly UpdatingPrimaryValue PlainTextValue = new();
    private readonly UpdatingKeyValue ContentType = new("contenttype");
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, PlainTextValue).IsRereadRequired(out string? plainText);
        (constants, ContentType).IsRereadRequired(() => "*/*", out var contentType);

        if (interaction.TryGetClosest<ISinkingInteraction>(out var sinkInteraction) && 
            sinkInteraction != null)
        {
            sinkInteraction.WriteSegment(plainText ?? "", contentType);
            OnThen?.Invoke(this, interaction);
        }
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "unable to find text sink"));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

