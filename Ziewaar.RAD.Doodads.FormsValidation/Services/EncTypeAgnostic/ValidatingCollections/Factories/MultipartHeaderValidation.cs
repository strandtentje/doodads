using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;
using Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingMultipart;
using MultipartHeader =
    (string HeaderName, string HeaderValue, System.Collections.Generic.IReadOnlyDictionary<string, string> HeaderArgs);

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;

public static class MultipartHeaderValidation
{
    public static bool Run(
        string expectedName,
        object providedValue,
        out string failureReason,
        [NotNullWhen(true)] out ICountingEnumerator<byte>? data,
        [NotNullWhen(true)] out MultipartHeader? disposition,
        [NotNullWhen(true)] out MultipartHeader? contentType)
    {
        disposition = null;
        contentType = null;
        data = null;
        failureReason = "";
        if (providedValue is not ITaggedCountingEnumerator<byte> taggedByteEnumerator)
        {
            failureReason = "Byte stream not tagged with supplementary information";
            return false;
        }
        else if (taggedByteEnumerator.Tag is not IEnumerable<MultipartHeader> headerCollection ||
                 headerCollection?.ToArray() is not MultipartHeader[] headerArray ||
                 headerArray.Length < 1)
        {
            failureReason = "Supplementary information wasn't headers";
            return false;
        }
        else if (
            headerArray?.Where(x =>
                        x.HeaderName.Equals("content-disposition", StringComparison.OrdinalIgnoreCase))
                    .ToArray() is not MultipartHeader[]
                dispositionHeaderArray ||
            dispositionHeaderArray.Length < 1)
        {
            failureReason = "No content disposition";
            return false;
        }
        else if (!dispositionHeaderArray[0].HeaderValue.Equals("form-data", StringComparison.OrdinalIgnoreCase))
        {
            failureReason = "Content disposition wasn't form-data";
            return false;
        }
        else if (!dispositionHeaderArray[0].HeaderArgs.TryGetValue("name", out string? fieldName))
        {
            failureReason = "Field had no name";
            return false;
        }
        else if (!expectedName.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
        {
            failureReason = "Field name did not match";
            return false;
        }
        else
        {
            data = taggedByteEnumerator;
            disposition = dispositionHeaderArray[0];
            contentType = headerArray.Where(x =>
                    x.HeaderName.Equals("content-type", StringComparison.OrdinalIgnoreCase)).Concat([
                    (
                        "Content-Type", "text/plain", new Dictionary<string, string>(1) { ["charset"] = "utf-8" })
                ])
                .ToArray()[0];
            return true;
        }
    }
}