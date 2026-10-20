using Ziewaar.Web.Uploadserver;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.FastMultipart;

public class UploadForm : BasicService
{
    private static readonly UploadBoxRepository Repository = new UploadBoxRepository();
    public override event CallForInteraction? OnThen
    {
        add => base.OnThen += value;
        remove => base.OnThen -= value;
    }

    public override event CallForInteraction? OnElse
    {
        add => base.OnElse += value;
        remove => base.OnElse -= value;
    }

    public event CallForInteraction? SinkForm;

    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        
    }
}