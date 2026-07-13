#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

[Category("Printing & Formatting")]
[Title("Outputs text to the closest output stream")]
[Description("""
             Almost behaves like you would expect. For example, when used in conjunction with a webserver,
             it will simply write the configured text to the Response body, attempting to bring along its content type.
             """)]
public class Print : IService
{
    [PrimarySetting("Text to print to output")]
    private readonly UpdatingPrimaryValue PlainTextValue = new();
    [NamedSetting("contenttype", "Optionally, content type filter this text fits with. Defaults to */*.")]
    private readonly UpdatingKeyValue ContentType = new("contenttype");

    private string PlainText = "";

    [EventOccasion("Happens when the print was successful; preserves the interaction for more printing.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when not used in conjunction with something has an output stream")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, PlainTextValue).IsRereadRequired(out object? plainText))
        {
            this.PlainText = plainText?.ToString() ?? "";
        }
        (constants, ContentType).IsRereadRequired(() => "*/*", out var contentType);

        var foundInteraction = interaction.TryGetClosest<IInteraction>(
            out var targetInteraction, x => x is ICheckUpdateRequiredInteraction || x is ISinkingInteraction);

        if (!foundInteraction)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "unable to find sink or update-sensitive interaction"));            
        }
        else if (targetInteraction is ICheckUpdateRequiredInteraction checkUpdateRequiredInteraction)
        {
            checkUpdateRequiredInteraction.IsRequired =
                checkUpdateRequiredInteraction.Original.LastSinkChangeTimestamp != constants.PrimaryLog;            
        }
        else if (targetInteraction is ISinkingInteraction sinkInteraction)
        {
            try
            {
                sinkInteraction.SinkTrueContentType = contentType;
                sinkInteraction.WriteSegment(this.PlainText, contentType);
            }
            catch (ContentTypeMismatchException ex)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, ex));
                return;
            }
            sinkInteraction.LastSinkChangeTimestamp = constants.PrimaryLog;
            OnThen?.Invoke(this, interaction);
        }
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "unable to find text sink"));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

