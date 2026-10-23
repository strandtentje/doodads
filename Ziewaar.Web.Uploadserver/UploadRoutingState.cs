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
