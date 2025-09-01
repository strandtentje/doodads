using System.Text;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public class StreamCharEnumerator(Stream stream, Dictionary<string, string> headers) : IEnumerator<char>
{
    private readonly Decoder Decoder = headers.GetEncodingOrDefault(Encoding.UTF8).GetDecoder();
    private const int BufferSize = 512;
    private readonly byte[] ByteBuffer = new byte[BufferSize];

    private readonly char[] CharBuffer =
        new char[headers.GetEncodingOrDefault(Encoding.UTF8).GetMaxCharCount(BufferSize)];

    private int CharIndex = 0;
    private int CharCount = 0;
    private char CurrentCharacter;
    public char Current => CurrentCharacter;
    object IEnumerator.Current => CurrentCharacter;

    public bool MoveNext()
    {
        if (CharIndex < CharCount)
        {
            CurrentCharacter = CharBuffer[CharIndex++];
            return true;
        }

        int bytesRead = stream.Read(ByteBuffer, 0, ByteBuffer.Length);
        if (bytesRead == 0) return false;

        CharCount = Decoder.GetChars(ByteBuffer, 0, bytesRead, CharBuffer, 0);
        CharIndex = 0;

        if (CharCount == 0) return false;

        CurrentCharacter = CharBuffer[CharIndex++];
        return true;
    }

    public void Reset() => throw new NotSupportedException();
    public void Dispose() { stream.Dispose(); }
}