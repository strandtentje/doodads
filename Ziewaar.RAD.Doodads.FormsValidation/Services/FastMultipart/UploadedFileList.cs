namespace Ziewaar.RAD.Doodads.FormsValidation.Services.FastMultipart;

[Category("Input & Validation")]
[Title("Iterate through uploaded files")]
[Description("Use after UploadedFileList to iterate through the destionation paths of uploaded files")]
public class UploadedFileList : IteratingService
{
    protected override bool RunElse => false;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        BasicException.ForCustom(repeater, out FinishedUpload finished);
        return finished.Files.Select(repeater.AppendRegister);
    }
}