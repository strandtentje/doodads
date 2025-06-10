#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

public class ConstantTextSource : IService
{
    private readonly UpdatingPrimaryValue PlainTextValue = new();
    private readonly UpdatingKeyValue ContentType = new("contenttype");
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, PlainTextValue).IsRereadRequired(out string? plainText);
        (constants, ContentType).IsRereadRequired(() => "*/*", out var contentType);

        if (interaction.TryGetClosest<ISinkingInteraction>(out var sinkInteraction) && 
            sinkInteraction != null)
        {
            sinkInteraction.WriteSegment(plainText ?? "", contentType);
        }
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "unable to find text sink"));
        }
    }
}

