namespace Ziewaar.Web.Uploadserver;

[Flags]
public enum UploadRoutingState
{
    Failure = 0b1000_0000_0000,
    Success = 0b0000_0000_0000,
    MakeBox = 0b0100_0000_0000,

    NoUploadMustMake = 1 | Failure | MakeBox,
    ExpiredMustMake = 4 | Failure | MakeBox,
    TaintedMustMake = 5 | Failure | MakeBox,
    
    BadMethod = 2 | Failure,
    BadRequest = 3 | Failure,
    BadFunction = 6 | Failure,
    BadStart = 7 | Failure,
    ConflictingUpload = 8 | Failure,
    TooBig = 9 | Failure,
    UnknownFailure = 10 | Failure,
    UploadFailed = 11 | Failure,

    UploadFunctionOk = 50 | Success,
    FormOk = 51 | Success,
    ProgressOk = 52 | Success,
    UploadDone = 53 | Success,
}

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

    public UploadRoutingState Route(HttpListenerRequest req, HttpListenerResponse rsp) =>
        req.HttpMethod.ToUpper() switch
        {
            "GET" => RouteGet(req, rsp),
            "POST" => RoutePost(req),
            _ => UploadRoutingState.BadMethod
        };

    private static readonly string EmptyGuid = Guid.Empty.ToString();

    private bool ValidateUploadFunction(
        HttpListenerRequest req,
        out string functionId, out UploadBox selectedUploadBox, out UploadRoutingState routingState)
    {
        var sessionId = this.SessionSourcer.GetSessionIdFor(req);
        var uploadBoxId = req.QueryString["uploadBoxId"];
        functionId = req.QueryString["functionId"];
        selectedUploadBox = UploadBox.CreateOrphan("", Array.Empty<string>());
        if (string.IsNullOrEmpty(uploadBoxId) ||
            string.IsNullOrEmpty(functionId))
        {
            routingState = UploadRoutingState.NoUploadMustMake;
            return false;
        }

        if (uploadBoxId.Length != EmptyGuid.Length ||
            functionId.Length < 4 ||
            functionId.Length > 10)
        {
            routingState = UploadRoutingState.BadRequest;
            return false;
        }
        else if (!UploadBoxes.TryPeek(sessionId, uploadBoxId,
                     out selectedUploadBox))
        {
            routingState = UploadRoutingState.ExpiredMustMake;
            return false;
        }
        else
        {
            functionId = functionId.Trim().ToUpper();
            routingState = UploadRoutingState.UploadFunctionOk;
            return true;
        }
    }

    private UploadRoutingState RouteGet(HttpListenerRequest req, HttpListenerResponse rsp)
    {
        if (!ValidateUploadFunction(req, out var functionId,
                out var selectedUploadBox, out var state))
            return state;

        switch (functionId)
        {
            case "PAGE" when !selectedUploadBox.ValidateUntainted():
                return UploadRoutingState.TaintedMustMake;

            case "PAGE":
                var placeHolders = new UploadBoxSpecificResolver(selectedUploadBox, FallbackResolver);
                DynamicPages.UploadForm.Render(rsp, placeHolders);
                return UploadRoutingState.FormOk;

            case "PROGRESS":
                ProgressGrapher.Run(selectedUploadBox, rsp);
                return UploadRoutingState.ProgressOk;

            default:
                return UploadRoutingState.BadFunction;
        }
    }

    private UploadRoutingState RoutePost(HttpListenerRequest req)
    {
        if (!ValidateUploadFunction(req, out var functionId,
                out var selectedUploadBox, out var urs))
            return urs;

        if (functionId != "UPLOAD") return UploadRoutingState.BadFunction;

        try
        {
            using (selectedUploadBox)
            {
                if (this.Receiver.TryValidateUpload(req.ContentType, req.ContentLength64,
                        selectedUploadBox, out var state))
                    return this.Receiver.TryReceiveUpload(req.InputStream, selectedUploadBox)
                        ? UploadRoutingState.UploadDone
                        : UploadRoutingState.UploadFailed;

                return state switch
                {
                    PreValidationState.BadStart => UploadRoutingState.BadStart,
                    PreValidationState.AlreadyUploading => UploadRoutingState.ConflictingUpload,
                    PreValidationState.TooBig => UploadRoutingState.TooBig,
                    _ => UploadRoutingState.UnknownFailure
                };
            }
        }
        finally
        {
            this.UploadBoxes.Remove(selectedUploadBox);
        }
    }
}