#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Generate;

[Category("Printing & Formatting")]
[Title("Sink binary data and output it as a base64 data url")]
[Description("""
    Hook up something that produces binary data with a mime type, typically some image or file reader, to the OnThen.
    OnElse will then have base64 encoded data url in its register.
    """)]
public class DataUrl : IService
{
    [PrimarySetting("Force the content type of the data url regardless of what was sunk")]
    private readonly UpdatingPrimaryValue ForceContentTypeConst = new();
    private string? ForceMimeTo;
    [EventOccasion("Happens after the conversion")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Sink binary data here.")]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ForceContentTypeConst).IsRereadRequired(out string? candidateMime)  && candidateMime != null)
        {
            this.ForceMimeTo = candidateMime;
        }
        string GetUrlText()
        {
            using (var bsi = new BinarySinkingInteraction(interaction))
            {
                OnElse?.Invoke(this, bsi);
                return DataUrlUtil.ToBase64DataUrl(bsi.SinkBuffer, ForceMimeTo ?? bsi.SinkTrueContentType ?? "application/octet-stream");
            }
        };
        OnThen?.Invoke(this, new CommonInteraction(interaction, GetUrlText()));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
