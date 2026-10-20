namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface IPlaceholderResolver
{
    long GetByteLengthFor(string p);

    void WriteBytesFor(string p, System.IO.Stream stream);
}