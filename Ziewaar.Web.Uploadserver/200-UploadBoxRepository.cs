namespace Ziewaar.Web.Uploadserver;

public class UploadBoxRepository
{
    private readonly object UploadBoxesLock = new object();
    private readonly Dictionary<UploadBox, UploadBox> UploadBoxes 
        = new Dictionary<UploadBox, UploadBox>(new UploadBoxComparerById());

    public UploadBox Enter(string sessionId, string path, 
        string[] acceptedFields)
    {
        lock (UploadBoxesLock)
        {
            var newBox = UploadBox.CreateFresh(sessionId, path, acceptedFields);
            if (UploadBoxes.ContainsKey(newBox))
                throw new InvalidOperationException(
                    "Unexpectedly created a duplicate upload box");
            UploadBoxes[newBox] = newBox;
            return newBox;
        }
    }

    public bool TryPeek(string sessionId, string uploadId, out UploadBox result)
    {
        lock (UploadBoxesLock)
        {
            result = UploadBox.CreateFinder(sessionId, uploadId);
            return UploadBoxes.TryGetValue(result, out result);
        }
    }

    public void Remove(UploadBox u)
    {
        lock (UploadBoxesLock)
        {
            UploadBoxes.Remove(u);
            u.Dispose();
        }
    }
}