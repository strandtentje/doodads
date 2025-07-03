#nullable enable
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

namespace Ziewaar.RAD.Doodads.CommonComponents;

[Category("Output to Sink")]
[Title("Template from Text service")]
[Description("""
             This is a shorthand for Template():Print(), and as such 
             will template using the contents of the text provided in the primary 
             constant. 
             """)]
public class Format : IService
{
    [EventOccasion("For further templating text, after the primary setting has been read.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When a template value is unknown, starts sinking text here")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when an IO error happened")]
    public event CallForInteraction? OnException;

    private readonly Print Printer = new();
    private readonly Template TemplatingService = new();

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        void HandleTemplateRequest(object sender, IInteraction templateRequestInteraction)
        {
            TemplatingService.OnThen -= HandleTemplateRequest;
            Printer.Enter(constants, templateRequestInteraction);
        }

        TemplatingService.OnThen += HandleTemplateRequest;
        TemplatingService.Enter(constants, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
