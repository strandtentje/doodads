#nullable enable
using System.ComponentModel.Design;
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
[Category("Output to Sink")]
[Title("Template from File service")]
[Description("""
             This is a shorthand for Template():PrintContent(), and as such 
             will template using the contents of the file provided in the primary 
             constant. 
             """)]
public class FileTemplate : IService
{
    PrintContent ContentPrinter = new();
    Template TemplatingService = new();
    public FileTemplate()
    {
        ContentPrinter.OnException += this.OnException;
        ContentPrinter.OnThen += this.OnThen;
        TemplatingService.OnException += this.OnException;
        TemplatingService.OnElse += this.OnElse;
        TemplatingService.OnThen += this.HandleTemplateRequest;
    }
    private void HandleTemplateRequest(object sender, IInteraction interaction)
    {
        if (this.CurrentConstants == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Unlikely error occurred"));
            return;
        }
        ContentPrinter.Enter(this.CurrentConstants, interaction);
    }
    [PrimarySetting("File to template from")]
    // ReSharper disable once UnusedMember.Local
    private readonly UpdatingPrimaryValue ContentFile = new();
    private StampedMap? CurrentConstants;
    [EventOccasion("For further templating text, after the file has been read.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When a template value is unknown, starts sinking text here")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when an IO error happened")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        this.CurrentConstants = constants;
        TemplatingService.Enter(constants, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
