using Ejije.Logging;
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly.HtmlFields;

public class ValidatingRadio(HtmlNode node) : IValidatingInputFieldInSet
{
    public event EventHandler<(string oldName, string newName)>? NameChanged;
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
        get => 1;
        set { }
    }
    private IEnumerable<string> ValidValues { get; set; } = [];
    public List<IValidatingInputField> AltValidators { get; } = new();
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "radio")
            return false;
        if (node.GetInputName() is not string)
            return true;
        if (node.GetInputValue() is string radioValue)
            set.Merge(new ValidatingRadio(node)
            {
                IsRequired = node.IsRequired(),
                ValidValues = [radioValue]
            });
        return true;
    }
    public bool TryValidate(string[] submittedValue, out IEnumerable result)
    {
        if (submittedValue.Length == 1 && ValidValues.Contains(submittedValue.Single()) ||
            submittedValue.Length == 0 && !IsRequired)
        {
            result = submittedValue;
            return true;
        }
        else
        {
            result = Enumerable.Empty<object>();
            return false;
        }
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        if (otherFieldInSet is ValidatingRadio otherRadio)
        {
            ValidValues = ValidValues.Concat(otherRadio.ValidValues);
            return true;
        }
        return false;
    }
}

public class ValidatingFile(HtmlNode node, SortedSet<(string mimeType, string extension)> acceptedFileTypes,
    int maxFileCount, long maxFileSize) : IValidatingInputFieldInSet
{
    public event EventHandler<(string oldName, string newName)>? NameChanged;
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
    private IEnumerable<string> ValidValues { get; set; } = [];
    public List<IValidatingInputField> AltValidators { get; } = new();
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "file")
            return false;
        if (node.GetInputName() is not string)
            return true;

        var isMultiple = node.GetAttributes().Any(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, "multiple"));
        var maxCount = int.TryParse(node.GetMax() ?? "-1", CultureInfo.InvariantCulture, out int candidateMax) ?
            candidateMax : -1;
        var maxLength = node.GetAttributeValue("maxlength", "4mb").ParseByteSize();
        var accept = node.GetAttributeValue("accept", "").Split(',', (StringSplitOptions)3).ToArray();
        var acceptExt = accept.Where(x => x.StartsWith('.') && !x.Contains('/')).Select(x => x.Trim('.')).ToArray();
        var acceptMime = accept.Where(x => x.Contains('/') && !x.StartsWith('.')).ToArray();
        SortedSet<(string mimeType, string extension)> acceptedFileTypes = new();

        foreach (var ext in acceptExt)
            if (MimeMapping.ExtensionToMimeType.TryGetValue(ext, out var mime))
                acceptedFileTypes.Add((mime.MimeType, ext));

        foreach (var mime in acceptMime)
            if (MimeMapping.MimeTypeToExtension.TryGetValue(mime, out var ext))
                acceptedFileTypes.Add((mime, ext));

        set.Merge(new ValidatingFile(node, acceptedFileTypes, maxCount, maxLength)
        {
            IsRequired = node.IsRequired(),
        });

        return true;
    }

    private static readonly Log 
        FileNotFound = Log.Oops(
            """Uploaded {file} disappeared and could not be validated."""),
        FileTooBig = Log.Warn(
            """Uploaded {file} was too big, removing."""),
        ExtensionsToMime = Log.Warn(
            """Unrecognized extension for {file}""")
        ;

    public bool TryValidate(string[] submittedValue, out IEnumerable result)
    {
        var infos = submittedValue.Select(x => new FileInfo(x)).ToArray();
        var disappearedInfos = infos.Where(x => !x.Exists).ToArray();
        List<FileInfo> acceptedFiles = new List<FileInfo>();

        foreach (var item in infos)
        {
            if (!item.Exists)
            {
                Log.Post(FileNotFound, item);
                break;
            } else if (item.Length > maxFileSize)
            {
                Log.Post(FileTooBig, item);
                break;
            }
            var trimExt = item.Extension.TrimStart('.');
            if (MimeMapping.ExtensionToMimeType.TryGetValue(trimExt, out var mime))
            {
                Log.Post(ExtensionsToMime, item);
                break;
            }
        }


        if (submittedValue.Length == 1 && ValidValues.Contains(submittedValue.Single()) ||
            submittedValue.Length == 0 && !IsRequired)
        {
            result = submittedValue;
            return true;
        }
        else
        {
            result = Enumerable.Empty<object>();
            return false;
        }
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        if (otherFieldInSet is ValidatingRadio otherRadio)
        {
            ValidValues = ValidValues.Concat(otherRadio.ValidValues);
            return true;
        }
        return false;
    }
}