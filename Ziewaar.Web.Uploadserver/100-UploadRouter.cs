namespace Ziewaar.Web.Uploadserver;

public class UploadRouter
{
    private readonly ISessionSourcer SessionSourcer;
    private readonly IPlaceholderResolver FallbackResolver;
    private readonly IProgressGrapher ProgressGrapher;
    private readonly IUploadReceiver Receiver;
    private readonly UploadBoxRepository UploadBoxes;
    public UploadRouter(
        ISessionSourcer sessionSourcer,
        IPlaceholderResolver fallbackResolver,
        IProgressGrapher grapher,
        IUploadReceiver uploadReceiver,
        UploadBoxRepository uploadBoxes)
    {
        this.SessionSourcer = sessionSourcer;
        this.FallbackResolver = fallbackResolver;
        this.ProgressGrapher = grapher;
        this.Receiver = uploadReceiver;
        this.UploadBoxes = uploadBoxes;
    }

    public void Route(HttpListenerRequest req, HttpListenerResponse rsp)
    {
        switch (req.HttpMethod.ToUpper())
        {
            case "GET":
                RouteGet(req, rsp);
                break;
            case "POST":
                RoutePost(req, rsp);
                break;
            default:
                rsp.WriteUtf8Bytes(405, StaticPages.Instance.Pg100BadMethod);
                break;
        }
    }
    private static readonly string EmptyGuid = Guid.Empty.ToString();

    private bool ValidateUploadFunction(
        HttpListenerRequest req, HttpListenerResponse rsp,
        out string functionId, out UploadBox selectedUploadBox)
    {
        var sessionId = this.SessionSourcer.GetSessionIdFor(req);
        var uploadBoxId = req.QueryString["uploadBoxId"];
        functionId = req.QueryString["functionId"];
        selectedUploadBox = null;
        if (string.IsNullOrEmpty(uploadBoxId) ||
            uploadBoxId.Length != EmptyGuid.Length ||
            string.IsNullOrEmpty(functionId) ||
            functionId.Length < 4 ||
            functionId.Length > 10)
        {
            rsp.WriteUtf8Bytes(400, StaticPages.Instance.Pg110BadGetRequest);
            return false;
        }
        else if (!UploadBoxes.TryPeek(sessionId, uploadBoxId, 
                     out selectedUploadBox))
        {
            rsp.WriteUtf8Bytes(410, StaticPages.Instance.Pg120UploaderGone);
            return false;
        }
        else
        {
            functionId = functionId.Trim().ToUpper();
            return true;
        }
    }

    private void RouteGet(HttpListenerRequest req, HttpListenerResponse rsp)
    {
        string functionId;
        UploadBox selectedUploadBox;
        if (!ValidateUploadFunction(req, rsp, out functionId, 
                out selectedUploadBox))
            return;

        switch (functionId)
        {
            case "PAGE":
                if (selectedUploadBox.ValidateUntainted())
                {
                    var placeHolders = new UploadBoxSpecificResolver(
                        selectedUploadBox, FallbackResolver);
                    DynamicPages.UploadForm.Render(
                        rsp, placeHolders);
                }
                else
                {
                    throw new NotImplementedException(
                        "TODO: Implement redirect upon active upload");
                }
                break;
            case "PROGRESS":
                this.ProgressGrapher.Run(selectedUploadBox, rsp);
                break;
            case "COMBO":
                var placeHolders2 = new UploadBoxSpecificResolver(
                    selectedUploadBox, FallbackResolver);
                DynamicPages.ProgressPage.Render(
                    rsp, placeHolders2);

                break;
            default:
                rsp.WriteUtf8Bytes(400, 
                    StaticPages.Instance.Pg130BadGetFunction);
                break;
        }
    }
    private void RoutePost(HttpListenerRequest req, HttpListenerResponse rsp)
    {
        if (!ValidateUploadFunction(req, rsp, out var functionId, 
                out var selectedUploadBox))
            return;

        switch (functionId)
        {
            case "UPLOAD":
                // we will always dispose the uploadBox after upload.
                using (selectedUploadBox)
                {
                    if (!this.Receiver.TryValidateUpload(req.ContentType, req.ContentLength64,
                            selectedUploadBox, out var state))
                    {
                        switch (state)
                        {
                            case PreValidationState.BadStart:
                                rsp.WriteUtf8Bytes(400, StaticPages.Instance.Pg140BadUploadStart);
                                return;
                            case PreValidationState.AlreadyUploading:
                                rsp.WriteUtf8Bytes(409, StaticPages.Instance.Pg150ConflictingUpload);
                                return;
                            case PreValidationState.TooBig:
                                rsp.WriteUtf8Bytes(413, StaticPages.Instance.Pg160UploadTooBig);
                                return;
                            case PreValidationState.NoFaults:
                            default:
                                return;
                        }
                    }

                    if (!this.Receiver.TryReceiveUpload(req.InputStream, selectedUploadBox))
                        return;
                }
                break;
            default:
                break;
        }
    }
}