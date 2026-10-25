namespace Ziewaar.Web.Uploadserver.FieldResults.Unfinished;

public class MultipartMultipleFileResult(string fieldName, string targetDirectory, string innerBoundary)
    : IMultipartComponentResult
{
    public readonly string FieldName = fieldName, TargetDirectory = targetDirectory, InnerBoundary = innerBoundary;
    public readonly List<MultipartSingleFileResult> UnfinishedSubfiles = new();
    private MultipartSingleFileResult? CurrentFile;
    private string? _backingFault;
    public bool IsRetired { get; private set; }

    private IEnumerable<string> GetSubFaults()
    {
        return UnfinishedSubfiles.Select(x => x.Fault).
            OfType<string>().Where(x => !string.IsNullOrEmpty(x));
    }

    public string Name => FieldName;

    public bool TryPush(string contentType, string fileName)
    {
        if (IsRetired)
        {
            var additionalFault = "Cannot add file; sub boundary retired.";
            if (_backingFault == null)
                _backingFault = additionalFault;
            else
                _backingFault += "\r\n" + additionalFault;
            return false;
        }

        if (CurrentFile != null)
            UnfinishedSubfiles.Add(CurrentFile);
        return MultipartSingleFileResult.TryCreateFor(
            this.FieldName, this.TargetDirectory,
            fileName, contentType, out CurrentFile);
    }

    public void GiveBinaryData(byte[] data, int start, int length)
    {
        if (length <= 0) return;
        if (CurrentFile != null && !IsRetired)
            CurrentFile.GiveBinaryData(data, start, length);
        else
            _backingFault = "Receiving binary data while no inner " +
                            "file header was provided";
    }

    public IMultipartFinishedComponentResult Finish()
    {
        if (CurrentFile != null)
            UnfinishedSubfiles.Add(CurrentFile);
        CurrentFile = null;
            
        var files = new List<IMultipartFinishedComponentResult>();
        foreach (var item in UnfinishedSubfiles)
        {
            files.Add(item.Finish());
        }

        return new MultipartFinishedMultifileResult(this.FieldName, [
            .. files.OfType<IMultipartFinishedComponentResult>().Select(x => x.Values.ElementAtOrDefault(0))
                .OfType<string>().Where(x => !string.IsNullOrEmpty(x))
        ]);
    }

    public string Fault
    {
        get {
            StringBuilder faultBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(this._backingFault))
                faultBuilder.AppendLine(_backingFault);
            foreach (var f in GetSubFaults())
                faultBuilder.AppendLine(f);
            return faultBuilder.ToString();
        } 
    }

    public bool IsExpectingContent => !this.IsRetired && CurrentFile is { IsExpectingContent: true };
    
    public void Dispose()
    {
        if (CurrentFile != null)
            UnfinishedSubfiles.Add(CurrentFile);
        CurrentFile = null;
        foreach (var item in UnfinishedSubfiles)
        {
            item.Dispose();
        }
    }

    internal void Retire()
    {
        this.IsRetired = true;
    }

    public override string ToString()
    {
        return string.Format("MultipartMultipleFileResult({0})", 
            this.TargetDirectory);
    }
}