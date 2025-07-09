using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class HtmlForm : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ISinkingInteraction>(out var targetSink) || targetSink == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "may only do this where there's another sink"));
            return;
        }

        var tsi = new TextSinkingInteraction(interaction, textEncoding: targetSink.TextEncoding);
        HtmlDocument doc = new();
        doc.Load(tsi.SinkBuffer, targetSink.TextEncoding);
        doc.DocumentNode.SelectNodes()
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}