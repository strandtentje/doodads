#nullable enable
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

namespace Ziewaar.RAD.Doodads.CommonComponents;
[Category("Output to Sink")]
[Title("Template from File service")]
[Description("""
             This is a shorthand for Template():PrintContent(), and as such 
             will template using the contents of the file provided in the primary 
             constant. 
             """)]
public class FileTemplate : IService
{
    public FileTemplate()
    {
        ContentPrinter.OnException += this.OnException;
        ContentPrinter.OnThen += this.OnThen;
        TemplatingService.OnException += this.OnException;
        TemplatingService.OnElse += this.OnElse;
    }
    
    private readonly PrintContent ContentPrinter = new();
    private readonly Template TemplatingService = new();
    
    [EventOccasion("For further templating text, after the file has been read.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When a template value is unknown, starts sinking text here")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when an IO error happened")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        void HandleTemplateRequest(object sender, IInteraction templateRequestInteraction)
        {
            TemplatingService.OnThen -= HandleTemplateRequest;
            ContentPrinter.Enter(constants, templateRequestInteraction);
        }
        
        TemplatingService.OnThen += HandleTemplateRequest;
        TemplatingService.Enter(constants, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}