namespace Ziewaar.Web.Uploadserver.Pages;

public class UploadBoxSpecificResolver : IPlaceholderResolver
{
    private readonly UploadBox BackingBox;
    private readonly IPlaceholderResolver Fallback;
    private const string UPLOAD_BOX_ID = "uploadBoxId";

    public UploadBoxSpecificResolver(UploadBox backingBox,
        IPlaceholderResolver fallback)
    {
        this.BackingBox = backingBox;
        this.Fallback = fallback;
    }
    
    public long GetByteLengthFor(Encoding encoding, string p)
    {
        if (StringComparer.Ordinal.Equals(p, UPLOAD_BOX_ID))
            return encoding.GetByteCount(this.BackingBox.UploadBoxId);
        else
            return this.Fallback.GetByteLengthFor(encoding, p);
    }

    public void WriteBytesFor(IHttpLifecycleWrapper lifecycle, Encoding encoding, string p, Stream stream)
    {
        if (StringComparer.Ordinal.Equals(p, UPLOAD_BOX_ID))
        {
            var guidStringBytes = encoding.GetBytes(this.BackingBox.UploadBoxId);
            stream.Write(guidStringBytes, 0, guidStringBytes.Length);
        }
        else
            Fallback.WriteBytesFor(lifecycle, encoding, p, stream);
    }
}