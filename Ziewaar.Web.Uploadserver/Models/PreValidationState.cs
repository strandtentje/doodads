namespace Ziewaar.Web.Uploadserver.Models;

public enum PreValidationState
{
    NoFaults = 0,
    BadStart = 1,
    TooBig = 2,
    AlreadyUploading = 3,
}