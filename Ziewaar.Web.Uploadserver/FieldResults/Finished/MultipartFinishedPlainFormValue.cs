namespace Ziewaar.Web.Uploadserver.FieldResults.Finished;

public class MultipartFinishedPlainFormValue : 
    IMultipartFinishedComponentResult
{
    private readonly string _backingName;
    private readonly string[] _backingValue;
    public MultipartFinishedPlainFormValue(string name, string value)
    {
        _backingName = name;
        _backingValue = new string[] { value };
    }

    public string Name
    {
        get { return _backingName; } 
    }

    public string[] Values
    {
        get { return _backingValue; }
    }

    public bool IsFile
    {
        get { return false; }
    }
}