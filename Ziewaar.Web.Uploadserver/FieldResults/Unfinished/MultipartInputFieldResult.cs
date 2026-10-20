namespace Ziewaar.Web.Uploadserver.FieldResults.Unfinished;

public class MultipartInputFieldResult : IMultipartComponentResult
{
    private readonly string _name;
    private readonly List<byte> Data = new List<byte>();
    private string FinishedValue;

    public string Name { get { return _name; } }

    public MultipartInputFieldResult(string name)
    {
        this._name = name;
    }

    public void GiveBinaryData(byte[] data, int start, int length)
    {
        this.Data.AddRange(data.Skip(start).Take(length).ToArray());
    }

    public IMultipartFinishedComponentResult Finish()
    {
        var bytes = Data.ToArray();
        FinishedValue = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        return new MultipartFinishedPlainFormValue(this.Name, FinishedValue);
    }

    public string Fault
    {
        get { return null; }
    }

    public bool IsExpectingContent
    {
        get { return true; }
    }

    public void Dispose()
    {

    }

    public override string ToString()
    {
        return string.Format("MultipartInputFieldResult({0})", this.Name);
    }
}