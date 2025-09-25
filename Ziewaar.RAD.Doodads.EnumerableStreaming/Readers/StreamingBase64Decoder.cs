namespace Ziewaar.RAD.Doodads.EnumerableStreaming.Readers
{
    public class StreamingBase64Decoder(ICountingEnumerator<char> reader) : ICountingEnumerator<byte>
    {
        public bool AtEnd { get; private set; }
        public long Cursor { get; private set; }
        public string ErrorState { get; set; }
        public byte Current { get; private set; }
        object IEnumerator.Current => Current;
        private int byteBufferCursor = 0;
        private int byteBufferEndstop = 0;
        private byte[] dataBuffer = new byte[5 * 1024];
        private int base64Length = 0;
        private char[] base64Buffer = new char[5 * 1024];
        public bool MoveNext()
        {
            if (byteBufferCursor >= byteBufferEndstop)
            {
                base64Length = 0;

                while ((base64Length < 4 * 1024) && reader.MoveNext())
                    base64Buffer[base64Length++] = reader.Current;
        
                if ((base64Length % 4) != 0)
                {
                    ErrorState = "Base64 corrupt; not a multiple of 4 in length. Likely missing padding chars.";
                    return false;
                }

                if (!NonAllocatingBase64Decoder.TryDecode(base64Buffer, base64Length, dataBuffer, out byteBufferEndstop))
                {
                    ErrorState = "Base64 corrupt; likely wrong characters encountered.";
                    return false;
                }
            
                byteBufferCursor = 0;
            }

            if (byteBufferCursor < byteBufferEndstop)
            {
                Current = dataBuffer[byteBufferCursor++];
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
}