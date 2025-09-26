namespace Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;
public class StreamingBase64Decoder(ICountingEnumerator<char> reader) : ICountingEnumerator<byte>
{
    public bool AtEnd { get; private set; }
    public long Cursor { get; private set; }
    public string? ErrorState { get; set; }
    public byte Current { get; private set; }
    object IEnumerator.Current => Current;
    private int ByteBufferCursor;
    private int ByteBufferEndStop;
    private readonly byte[] DataBuffer = new byte[5 * 1024];
    private int Base64Length;
    private readonly char[] Base64Buffer = new char[5 * 1024];
    public bool MoveNext()
    {
        if (ByteBufferCursor >= ByteBufferEndStop)
        {
            Base64Length = 0;

            while ((Base64Length < 4 * 1024) && reader.MoveNext())
                Base64Buffer[Base64Length++] = reader.Current;
        
            if ((Base64Length % 4) != 0)
            {
                ErrorState = "Base64 corrupt; not a multiple of 4 in length. Likely missing padding chars.";
                return false;
            }

            if (!NonAllocatingBase64Decoder.TryDecode(Base64Buffer, Base64Length, DataBuffer, out ByteBufferEndStop))
            {
                ErrorState = "Base64 corrupt; likely wrong characters encountered.";
                return false;
            }
            
            ByteBufferCursor = 0;
        }

        if (ByteBufferCursor < ByteBufferEndStop)
        {
            Current = DataBuffer[ByteBufferCursor++];
            Cursor++;
            return true;
        } else
        {
            AtEnd = true;
            return false;
        }
    }
    public void Reset()
    {
        Current = 0;
        Cursor = 0;        
        AtEnd = false;
        ErrorState = null;
    }
    public void Dispose()
    {
        
    }
}