using Ejije.Logging;
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly.HtmlFields;

public class ValidatingFile(
    HtmlNode node,
    int maxFileCount,
    long maxFileSize) : IValidatingInputFieldInSet
{
    public event EventHandler<(string oldName, string newName)>? NameChanged;

    public required SortedSet<(string mimeType, string extension)> AcceptedFileTypes { get; set; }

    public string Name
    {
        get => node.GetInputName() ?? "";
        set
        {
            var oldName = node.GetInputName();
            node.SetInputName(value);
            if (oldName == null) return;
            NameChanged?.Invoke(this, (oldName, value));
        }
    }

    public bool IsRequired { get; private set; }
    public bool IsMaxUnbound => false;

    public int MinExpectedValues
    {
        get => IsRequired ? 1 : 0;
        set { }
    }

    public int MaxExpectedValues
    {
        get => maxFileCount;
        set { }
    }

    public List<IValidatingInputField> AltValidators { get; } = new();

    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "file")
            return false;
        if (node.GetInputName() is not string)
            return true;

        var isMultiple = node.GetAttributes().Any(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, "multiple"));
        var maxCount = int.TryParse(node.GetMax() ?? "-1", CultureInfo.InvariantCulture, out int candidateMax)
            ? candidateMax
            : -1;
        if (!isMultiple) maxCount = 1;
        var maxLength = node.GetAttributeValue("maxlength", "4mb").ParseByteSize();
        var accept = node.GetAttributeValue("accept", "").Split(',', (StringSplitOptions)3).ToArray();
        var acceptExt = accept.Where(x => x.StartsWith('.') && !x.Contains('/')).Select(x => x.Trim('.')).ToArray();
        var acceptMime = accept.Where(x => x.Contains('/') && !x.StartsWith('.')).ToArray();
        SortedSet<(string mimeType, string extension)> acceptedFileTypes = new(TupleComparer.OrdinalIgnoreCase);

        foreach (var ext in acceptExt)
            if (MimeMapping.ExtensionToMimeType.TryGetValue(ext, out var mime))
                acceptedFileTypes.Add((mime.MimeType, ext));

        foreach (var mime in acceptMime)
            if (MimeMapping.MimeTypeToExtension.TryGetValue(mime, out var ext))
                acceptedFileTypes.Add((mime, ext));

        set.Merge(new ValidatingFile(node, maxCount, maxLength)
        {
            IsRequired = node.IsRequired(),
            AcceptedFileTypes = acceptedFileTypes,
        });

        return true;
    }

    private static readonly Log
        TooManyFiles = Log.Warn("Too many files uploaded"),
        TooFewFiles = Log.Warn("Too few files uploaded"),
        FileNotFound = Log.Oops("Uploaded {file} disappeared and could not be validated."),
        FileTooBig = Log.Warn("Uploaded {file} was too big, removing."),
        ExtensionsToMime = Log.Warn("Unrecognized extension for {file}"),
        UnacceptableFile = Log.Warn("Filetype of upload to {file} was not accepted"),
        DeleteFailure = Log.Fail("Invalidated {file} delete {failure}");

    private IEnumerable<(Log? log, FileInfo? file)> EnumerateAcceptableFiles(string[] paths)
    {
        if (paths.Length < MinExpectedValues || (paths.Length == 0 && IsRequired))
            yield return (TooFewFiles, null);
        else if (paths.Length > MaxExpectedValues)
            yield return (TooManyFiles, null);
        else
            foreach (var path in paths)
            {
                var item = new FileInfo(path);
                var trimExt = item.Extension.TrimStart('.');

                if (!item.Exists)
                    yield return (FileNotFound, item);
                else if (item.Length > maxFileSize)
                    yield return (FileTooBig, item);
                else if (!MimeMapping.ExtensionToMimeType.TryGetValue(trimExt, out var mime))
                    yield return (ExtensionsToMime, item);
                else if (!AcceptedFileTypes.Contains((mime.MimeType, trimExt)))
                    yield return (UnacceptableFile, item);
                else
                    yield return (null, item);
            }
    }

    public bool TryValidate(string[] submittedValue, out IEnumerable result)
    {
        (Log? log, FileInfo? file)[]? validationResults = [.. EnumerateAcceptableFiles(submittedValue)];

        if (validationResults.Count(x => x.log == null) == submittedValue.Length)
        {
            result = validationResults.Select(x => x.file);
            return true;
        }

        foreach (var validationResult in validationResults)
        {
            if (validationResult.log != null)
                Log.Post(validationResult.log, validationResult.file);
            try
            {
                validationResult.file?.Delete();
            }
            catch (Exception ex)
            {
                Log.Post(DeleteFailure, validationResult.file, ex);
            }
        }

        result = Enumerable.Empty<object>();
        return false;
    }

    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet is not ValidatingFile otherFileBox)
            return false;
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");

        foreach (var otherType in otherFileBox.AcceptedFileTypes)
            this.AcceptedFileTypes.Add(otherType);
        return true;
    }
}