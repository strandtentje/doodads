using Ziewaar.Web.Uploadserver.FieldResults.Unfinished;

namespace Ziewaar.Web.Uploadserver;

public class CommonMultipartComponentIngestor : IMultipartComponentIngestor
{
    private readonly Stack<byte[]> Boundaries = new Stack<byte[]>();

    private readonly List<IMultipartComponentResult> FinishableResults =
        new List<IMultipartComponentResult>();

    private IMultipartComponentResult UnfinishedResult;
    private readonly string[] AcceptedFields;
    private readonly string TargetDirectory;
    private readonly IUploadValidator UploadValidator;

    public CommonMultipartComponentIngestor(byte[] boundary,
        string targetDirectory, string[] acceptedFields,
        IUploadValidator uploadValidator)
    {
        this.Boundaries.Push(boundary);
        this.TargetDirectory = targetDirectory;
        this.AcceptedFields = acceptedFields;
        this.UnfinishedResult = new FormStartComponentResult();
        this.UploadValidator = uploadValidator;
    }

    public bool IsExpectingContent
    {
        get { return UnfinishedResult.IsExpectingContent; }
    }

    public bool TryHaveBoundary(out byte[] boundary)
    {
        if (Boundaries.Any())
        {
            boundary = Boundaries.Peek();
            return true;
        }
        else
        {
            boundary = new byte[0];
            return false;
        }
    }

    public bool GiveHeaderString(string headerString)
    {
        var finish = FinishableResults.OfType<MultipartFinishedResult>().SingleOrDefault();

        if (finish != null)
            throw new InvalidOperationException("cannot finish this twice");

        if (!Boundaries.Any())
            throw new InvalidOperationException("No longer within boundaries");

        var headers = HttpHeaderCollection.Parse(headerString);

        var contentDisposition = headers.GetContentDisposition();
        var contentType = headers.GetContentType();
        var transferEncoding = headers.GetContentTransferEncoding();

        if (contentDisposition.HasMainValueCI("form-data"))
            return GiveFormDataHeader(
                contentDisposition, contentType, transferEncoding);

        if (contentDisposition.HasMainValueCI("file"))
            return GiveSubFileHeader(
                contentDisposition, contentType, transferEncoding);

        FinishableResults.Add(new MultipartComponentFaultyResult(
            "Unsupported or Invalid Content-Disposition"));
        return false;
    }

    private static readonly string[] IllegalNames = new string[]
    {
        "CON", "PRN", "AUX", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8",
        "COM9", "COM¹", "COM²", "COM³", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7",
        "LPT8", "LPT9", "LPT¹", "LPT²", "LPT³"
    };

    private bool GiveFormDataHeader(HttpHeader contentDispositionHeader,
        HttpHeader contentTypeHeader, HttpHeader transferEncodingHeader)
    {
        var fieldName = contentDispositionHeader.GetPropValueCI("name");
        if (!AcceptedFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase))
        {
            FinishableResults.Add(new MultipartComponentFaultyResult(
                "Missing name from content disposition"));
            return false;
        }

        var fileName = contentDispositionHeader.GetPropValueCI("filename");
        if (fileName == null && contentTypeHeader == null)
        {
            FinishableResults.Add(UnfinishedResult);
            UnfinishedResult = new MultipartInputFieldResult(fieldName);
            return true;
        }

        var contentType = contentTypeHeader.MainValue;
        var isMultiMixed = StringComparer.OrdinalIgnoreCase.Equals(
            contentType, "multipart/mixed");
        if (!string.IsNullOrEmpty(fileName) && !isMultiMixed)
        {
            if (!fileName.Intersect(Path.GetInvalidFileNameChars()).Any() &&
                !IllegalNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
            {
                FinishableResults.Add(UnfinishedResult);
                var fileCount = FinishableResults.OfType<MultipartSingleFileResult>().Count();
                fileCount += FinishableResults.OfType<MultipartMultipleFileResult>().Sum(x => x.UnfinishedSubfiles.Count);
                if (UploadValidator.TryValidate(contentType, fileCount + 1))
                {
                    if (MultipartSingleFileResult.TryCreateFor(
                            fieldName, TargetDirectory, fileName, contentType, out var ur))
                    {
                        UnfinishedResult = ur;
                        return true;
                    }
                    else
                    {
                        FinishableResults.Add(new MultipartComponentFaultyResult("File already exists"));
                        return false;
                    }
                }
                else
                {
                    FinishableResults.Add(new MultipartComponentFaultyResult(
                        "Too many files, or wrong file type."));
                    return false;
                }
            }
            else
            {
                FinishableResults.Add(new MultipartComponentFaultyResult(
                    "Filename contains invalid characters"));
                return false;
            }
        }

        
        if (isMultiMixed)
        {
            var innerBoundary = contentTypeHeader.GetPropValueCI("boundary")?.Trim();
            if (!string.IsNullOrEmpty(innerBoundary))
            {
                this.Boundaries.Push(Encoding.ASCII.GetBytes(innerBoundary));
                FinishableResults.Add(UnfinishedResult);
                UnfinishedResult = new MultipartMultipleFileResult(
                    fieldName, TargetDirectory, innerBoundary);
                return true;
            }
            else
            {
                FinishableResults.Add(new MultipartComponentFaultyResult(
                    "Unsupported"));
                return false;
            }
        }
        else
        {
            FinishableResults.Add(new MultipartComponentFaultyResult(
                "Incomplete"));
            return false;
        }
    }

    private bool GiveSubFileHeader(HttpHeader contentDispositionHeader,
        HttpHeader contentTypeHeader, HttpHeader transferEncodingHeader)
    {
        var asMultiFile = UnfinishedResult as MultipartMultipleFileResult;
        if (asMultiFile == null)
        {
            FinishableResults.Add(new MultipartComponentFaultyResult(
                "Multi-file multpart component without appropriate field"));
            return false;
        }

        var contentType = contentTypeHeader.MainValue;
        var fileCount = FinishableResults.OfType<MultipartSingleFileResult>().Count();
        fileCount += FinishableResults.OfType<MultipartMultipleFileResult>().Sum(x => x.UnfinishedSubfiles.Count);
        if (!UploadValidator.TryValidate(contentType, fileCount + 1))
        {
            FinishableResults.Add(new MultipartComponentFaultyResult(
                "Illegal content type or too many files"));
            return false;
        }

        var fileName = contentDispositionHeader.GetPropValueCI("filename").Trim();
        if (string.IsNullOrEmpty(fileName))
        {
            FinishableResults.Add(new MultipartComponentFaultyResult(
                "Missing filename on subfile"));
            return false;
        }

        if (asMultiFile.TryPush(contentType, fileName))
        {
            return true;
        }
        else
        {
            FinishableResults.Add(new MultipartComponentFaultyResult(
                "Failed to add subfile"));
            return false;
        }
    }

    private static readonly Log PassBinary = Log.Tech(Department.Upload,
        "Passing binary data between {start} and {end} to {field}");

    public void GiveBinaryData(byte[] bytes, int start, int count)
    {
        Log.Post(PassBinary, start, start + count, UnfinishedResult);
        UnfinishedResult.GiveBinaryData(bytes, start, count);
    }

    public bool TryRetireBoundary()
    {
        if (Boundaries.Count < 1)
        {
            FinishableResults.Add(new MultipartComponentFaultyResult(
                "Attempted to retire boundary without having boundaries"));
            return false;
        }

        var retiredBoundary = Boundaries.Pop();
        var asMultiFile = UnfinishedResult as MultipartMultipleFileResult;
        if (asMultiFile != null)
        {
            if (!StringComparer.Ordinal.Equals(
                    asMultiFile.InnerBoundary,
                    Encoding.ASCII.GetString(retiredBoundary)))
            {
                FinishableResults.Add(new MultipartComponentFaultyResult(
                    "Retiring boundary did not match open boundary"));
                return false;
            }
            else
            {
                asMultiFile.Retire();
                return true;
            }
        }

        if (Boundaries.Count > 0)
        {
            FinishableResults.Add(new MultipartComponentFaultyResult(
                "Unexpected boundaries unclosed"));
            return false;
        }

        return true;
    }

    public void Finish(
        IDictionary<string, IMultipartFinishedComponentResult> values,
        IList<string> faults)
    {
        FinishableResults.Add(UnfinishedResult);

        foreach (var item in FinishableResults)
            if (!string.IsNullOrEmpty(item.Fault))
                faults.Add(item.Fault);

        UnfinishedResult = new MultipartFinishedResult(faults);
        FinishableResults.Add(UnfinishedResult);

        if (!faults.Any())
        {
            foreach (var item in FinishableResults)
            {
                var usableResult =
                    item.Finish() as IMultipartFinishedComponentResult;
                if (usableResult != null)
                    values[usableResult.Name] = usableResult;
            }
        }
    }

    public void Dispose()
    {
        foreach (var item in FinishableResults)
        {
            item.Dispose();
        }
    }
}