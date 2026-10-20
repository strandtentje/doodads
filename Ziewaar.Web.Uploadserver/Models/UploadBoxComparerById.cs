namespace Ziewaar.Web.Uploadserver.Models;

public class UploadBoxComparerById : IEqualityComparer<UploadBox>
{
    public bool Equals(UploadBox x, UploadBox y)
    {
        return
            StringComparer.Ordinal.Equals(x.SessionId, y.SessionId) &&
            StringComparer.Ordinal.Equals(x.UploadBoxId, y.UploadBoxId);
    }

    public int GetHashCode(UploadBox obj)
    {
        return obj.SessionId.GetHashCode() ^ obj.UploadBoxId.GetHashCode();
    }
}