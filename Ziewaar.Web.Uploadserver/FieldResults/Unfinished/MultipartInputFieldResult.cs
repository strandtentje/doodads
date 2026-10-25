namespace Ziewaar.Web.Uploadserver.FieldResults.Unfinished;

public class MultipartInputFieldResult(string name) : IMultipartComponentResult
{
    private readonly List<byte> Data = [];
    public string Name { get; } = name;
    public void GiveBinaryData(byte[] data, int start, int length) => Data.AddRange(data.Skip(start).Take(length).ToArray());
    public IMultipartFinishedComponentResult Finish()
    {
        var bytes = Data.ToArray();
        var finishedValue = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        return new MultipartFinishedPlainFormValue(this.Name, finishedValue);
    }
    public string? Fault => null;
    public bool IsExpectingContent => true;
    public void Dispose() { }
    public override string ToString() => $"{nameof(MultipartInputFieldResult)}({this.Name})";
}