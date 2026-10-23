namespace Ziewaar.RAD.Doodads.FormsValidation.Services.FastMultipart;

public class UploadedFileList : IteratingService
{
    protected override bool RunElse => false;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        BasicException.ForCustom(repeater, out FinishedUpload finished);
        return finished.Files.Select(repeater.AppendRegister);
    }
}