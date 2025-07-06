#nullable enable
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

[Category("Output to Sink")]
[Title("Template from Text service")]
[Description("""
             This is a shorthand for Template():Print(), and as such 
             will template using the contents of the text provided in the primary 
             constant. 
             """)]
public class Format : IService
{
    [EventOccasion("After output was written.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When a template value is unknown, starts sinking text here")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when an IO error happened")]
    public event CallForInteraction? OnException;


    public void Enter(StampedMap constants, IInteraction interaction)
    {
        Print Printer = new();
        Template TemplatingService = new();
        void HandleTemplateRequest(object sender, IInteraction templateRequestInteraction)
        {
            TemplatingService.OnThen -= HandleTemplateRequest;
            Printer.Enter(constants, templateRequestInteraction);
        }

        TemplatingService.OnThen += HandleTemplateRequest;
        TemplatingService.OnElse += this.OnElse;
        TemplatingService.Enter(constants, interaction);
        this.OnThen?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
