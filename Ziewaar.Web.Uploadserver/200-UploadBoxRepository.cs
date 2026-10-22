using System.Runtime.InteropServices.ComTypes;

namespace Ziewaar.Web.Uploadserver;

public class UploadBoxRepository
{
    private readonly object UploadBoxesLock = new object();
    private readonly Dictionary<string, Dictionary<string, UploadBox>> UploadBoxesBySession = new(StringComparer.OrdinalIgnoreCase);

    public UploadBox Upsert(string sessionId, string path, string[] acceptedFields)
    {
        lock (UploadBoxesLock)
        {
            if (!UploadBoxesBySession.TryGetValue(sessionId, out var sessionBoxes))
                UploadBoxesBySession[sessionId] =
                    sessionBoxes = new Dictionary<string, UploadBox>(StringComparer.OrdinalIgnoreCase);

            if (sessionBoxes.Values.FirstOrDefault(x => x.IsVirgin) is not { } ub)
            {
                ub = UploadBox.CreateFresh(sessionId, path, acceptedFields);
                sessionBoxes[ub.UploadBoxId] = ub;
            }

            return ub;
        }
    }

    public bool TryPeek(string sessionId, string uploadId, out UploadBox result)
    {
        lock (UploadBoxesLock)
        {
            result = UploadBox.CreateFinder(sessionId, uploadId);
            if (!UploadBoxesBySession.TryGetValue(sessionId, out var uploadBoxes))
                return false;
            if (!uploadBoxes.TryGetValue(uploadId, out result))
                return false;
            return true;
        }
    }

    public void Remove(UploadBox u)
    {
        lock (UploadBoxesLock)
        {
            if (UploadBoxesBySession.TryGetValue(u.SessionId, out var sessionBoxes))
                sessionBoxes.Remove(u.UploadBoxId);
            u.Dispose();
        }
    }
}