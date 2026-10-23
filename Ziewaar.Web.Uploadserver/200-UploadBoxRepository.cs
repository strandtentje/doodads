using System.Runtime.InteropServices.ComTypes;

namespace Ziewaar.Web.Uploadserver;

public class UploadBoxRepository
{
    private readonly object UploadBoxesLock = new object();

    private readonly Dictionary<string, Dictionary<string, UploadBox>> BackingStore = new();


    public UploadBox Enter(string sessionId, string path,
        string[] acceptedFields)
    {
        lock (UploadBoxesLock)
        {
            if (!BackingStore.TryGetValue(sessionId, out var sessionUploadBoxes))
                BackingStore[sessionId] = sessionUploadBoxes = new Dictionary<string, UploadBox>();

            var untaintedCandidate =
                sessionUploadBoxes.Select(x => x.Value).FirstOrDefault(x =>
                    x.TargetDirectory == path && x.ValidateUntainted() &&
                    x.AcceptedFields.SequenceEqual(acceptedFields));
            if (untaintedCandidate == null)
            {
                untaintedCandidate = UploadBox.CreateFresh(sessionId, path, acceptedFields);
                sessionUploadBoxes[untaintedCandidate.UploadBoxId] = untaintedCandidate;
            }
            return untaintedCandidate;
        }
    }

    public bool TryPeek(string sessionId, string uploadId, out UploadBox result)
    {
        lock (UploadBoxesLock)
        {
            result = UploadBox.CreateOrphan("", Array.Empty<string>());
            return BackingStore.TryGetValue(sessionId, out var sessionUploadBoxes) &&
                   sessionUploadBoxes.TryGetValue(uploadId, out result);
        }
    }

    public void Remove(UploadBox u)
    {
        lock (UploadBoxesLock)
        {
            if (BackingStore.TryGetValue(u.SessionId, out var sessionBoxes))
            {
                sessionBoxes.Remove(u.UploadBoxId);
                if (sessionBoxes.Count < 1)
                    BackingStore.Remove(u.SessionId);
            }
            u.Dispose();
        }
    }
}