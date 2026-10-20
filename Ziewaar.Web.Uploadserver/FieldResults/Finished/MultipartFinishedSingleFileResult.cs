namespace Ziewaar.Web.Uploadserver.FieldResults.Finished;

public class MultipartFinishedSingleFileResult :
    IMultipartFinishedComponentResult
{
    private readonly string FieldName;
    private readonly string[] FilePaths;
    public MultipartFinishedSingleFileResult(
        string name, string filePath)
    {
        this.FieldName = name;
        this.FilePaths = new string[] { filePath, };
    }

    public string Name
    {
        get { return FieldName; }
    }

    public string[] Values
    {
        get { return FilePaths; }
    }

    public bool IsFile
    {
        get { return true; }
    }
}