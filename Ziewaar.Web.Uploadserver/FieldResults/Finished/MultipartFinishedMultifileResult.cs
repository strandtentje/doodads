namespace Ziewaar.Web.Uploadserver.FieldResults.Finished;

public class MultipartFinishedMultifileResult : 
    IMultipartFinishedComponentResult
{
    private readonly string _fieldName;
    private readonly string[] _filePaths;
    public MultipartFinishedMultifileResult(
        string fieldName, string[] filePaths)
    {
        this._fieldName = fieldName;
        this._filePaths = filePaths;
    }

    public string Name
    {
        get { return _fieldName; }
    }

    public string[] Values
    {
        get { return _filePaths; } 
    }

    public bool IsFile
    {
        get { return true; }
    }
}