namespace Ziewaar.Web.Uploadserver.Pages;

public class UploadBoxSpecificResolver : IPlaceholderResolver
{
    private readonly UploadBox BackingBox;
    private readonly IPlaceholderResolver Fallback;
    private readonly byte[] GuidAsUtf8;
    private const string UPLOAD_BOX_ID = "uploadBoxId";
    public UploadBoxSpecificResolver(UploadBox backingBox, 
        IPlaceholderResolver fallback)
    {
        this.BackingBox = backingBox;
        this.Fallback = fallback;
        this.GuidAsUtf8 = Encoding.UTF8.GetBytes(backingBox.UploadBoxId);
    }
    public long GetByteLengthFor(string p)
    {
        if (StringComparer.Ordinal.Equals(p, UPLOAD_BOX_ID))
            return this.GuidAsUtf8.Length;
        else
            return this.Fallback.GetByteLengthFor(p);
    }

    public void WriteBytesFor(string p, Stream stream)
    {
        if (StringComparer.Ordinal.Equals(p, UPLOAD_BOX_ID))
            stream.Write(GuidAsUtf8, 0, GuidAsUtf8.Length);
        else
            Fallback.WriteBytesFor(p, stream);
    }
}