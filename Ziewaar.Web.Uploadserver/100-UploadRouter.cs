namespace Ziewaar.Web.Uploadserver;

public class UploadRouter
{    
    private readonly IPlaceholderResolver FallbackResolver;
    private readonly IProgressGrapher ProgressGrapher;
    private readonly IUploadReceiver Receiver;
    private readonly UploadBoxRepository UploadBoxes;

    public UploadRouter(
        IPlaceholderResolver fallbackResolver,
        IProgressGrapher grapher,
        IUploadReceiver uploadReceiver,
        UploadBoxRepository uploadBoxes)
    {
        this.FallbackResolver = fallbackResolver;
        this.ProgressGrapher = grapher;
        this.Receiver = uploadReceiver;
        this.UploadBoxes = uploadBoxes;
    }

    public UploadRoutingState Route(IHttpLifecycleWrapper lifecycle, out IEnumerable<string> outputFiles)
    {
        outputFiles = Enumerable.Empty<string>();
        return lifecycle.UpperHttpMethod switch
        {
            "GET" => RouteGet(lifecycle),
            "POST" => RoutePost(lifecycle, out outputFiles),
            _ => UploadRoutingState.BadMethod
        };
    }

    private static readonly string EmptyGuid = Guid.Empty.ToString();

    private bool ValidateUploadFunction(
        IHttpLifecycleWrapper lifecycle,
        out string functionId, out UploadBox selectedUploadBox, out UploadRoutingState routingState)
    {
        var sessionId = lifecycle.SessionId;
        var uploadBoxId = lifecycle.GetFromQueryString("uploadBoxId");
        functionId = lifecycle.GetFromQueryString("functionId");
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

    private UploadRoutingState RouteGet(IHttpLifecycleWrapper lifecycle)
    {
        if (!ValidateUploadFunction(lifecycle, out var functionId,
                out var selectedUploadBox, out var state))
            return state;

        switch (functionId)
        {
            case "PAGE" when !selectedUploadBox.ValidateUntainted():
                return UploadRoutingState.TaintedMustMake;

            case "PAGE":
                var placeHolders = new UploadBoxSpecificResolver(selectedUploadBox, FallbackResolver);
                DynamicPages.UploadForm.Render(lifecycle, placeHolders);
                return UploadRoutingState.FormOk;

            case "PROGRESS":
                ProgressGrapher.Run(selectedUploadBox, lifecycle);
                return UploadRoutingState.ProgressOk;

            default:
                return UploadRoutingState.BadFunction;
        }
    }

    private UploadRoutingState RoutePost(IHttpLifecycleWrapper lifecycle, out IEnumerable<string> receivedPaths)
    {
        receivedPaths = Enumerable.Empty<string>();
        if (!ValidateUploadFunction(lifecycle, out var functionId,
                out var selectedUploadBox, out var urs))
            return urs;

        if (functionId != "UPLOAD") return UploadRoutingState.BadFunction;

        try
        {
            using (selectedUploadBox)
            {
                if (this.Receiver.TryValidateUpload(lifecycle.RequestContentType, lifecycle.RequestContentLength,
                        selectedUploadBox, out var state))
                {
                    var returnValue = this.Receiver.TryReceiveUpload(lifecycle.RequestStream, selectedUploadBox)
                        ? UploadRoutingState.UploadDone
                        : UploadRoutingState.UploadFailed;
                    
                    var singles = selectedUploadBox.Values.Values.OfType<MultipartFinishedSingleFileResult>()
                        .SelectMany(x => x.Values);
                    var multiples = selectedUploadBox.Values.Values.OfType<MultipartFinishedMultifileResult>()
                        .SelectMany(x => x.Values);
                    receivedPaths = singles.Concat(multiples);

                    return returnValue;
                }

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