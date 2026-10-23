namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface IPlaceholderResolver
{
    long GetByteLengthFor(Encoding encoding, string p);
    void WriteBytesFor(Encoding encoding, string p, Stream stream);
}