namespace Ziewaar.Web.Uploadserver.FieldResults.Unfinished;

public class MultipartMultipleFileResult : IMultipartComponentResult
{
    public readonly string FieldName, TargetDirectory, InnerBoundary;
    public readonly List<MultipartSingleFileResult> UnfinishedSubfiles = 
        new List<MultipartSingleFileResult>();
    private MultipartSingleFileResult CurrentFile = null;
    private string _backingFault = null;
    public bool IsRetired { get; private set; }

    private IEnumerable<string> GetSubFaults()
    {
        return UnfinishedSubfiles.Select(x => x.Fault).
            OfType<string>().Where(x => !string.IsNullOrEmpty(x));
    }

    public MultipartMultipleFileResult(
        string fieldName, string targetDirectory, string innerBoundary)
    {
        this.FieldName = fieldName;
        this.TargetDirectory = targetDirectory;
        this.InnerBoundary = innerBoundary;
    }

    public string Name
    {
        get { return FieldName; }
    }

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
        CurrentFile = MultipartSingleFileResult.CreateFor(
            this.FieldName, this.TargetDirectory,
            fileName, contentType);
        return true;
    }

    public void GiveBinaryData(byte[] data, int start, int length)
    {
        if ((CurrentFile == null || IsRetired) && length > 0)
        {
            _backingFault = "Receiving binary data while no inner " +
                            "file header was provided";
            return;
        }
        CurrentFile.GiveBinaryData(data, start, length);
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

        return new MultipartFinishedMultifileResult(
            this.FieldName, files.
                OfType<IMultipartFinishedComponentResult>().
                Select(x => x.Values.ElementAtOrDefault(0)).
                OfType<string>().
                Where(x => !string.IsNullOrEmpty(x)).ToArray());
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

    public bool IsExpectingContent
    {
        get { return !this.IsRetired &&
                     CurrentFile != null && CurrentFile.IsExpectingContent; }
    }

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