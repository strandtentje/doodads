namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public class StreamBoundarySlicer(Stream source, byte[] boundary)
{
    private readonly byte[] Pattern = boundary;
    private readonly int[] PrefixTable = PrefixSearchIndex.Build(boundary);

    private readonly Queue<byte> Buffer = new(); // Data confirmed safe to emit
    private readonly Queue<byte> Window = new(); // Current match window
    private int MatchIndex = 0;
    private bool EndReached = false;

    public int ReadUntilDelimiter(byte[] outputBuffer, int offset, int count)
    {
        if (EndReached)
            return 0;

        int bytesWritten = 0;

        while (bytesWritten < count)
        {
            // First drain the buffer (previously confirmed data)
            while (Buffer.Count > 0 && bytesWritten < count)
            {
                outputBuffer[offset + bytesWritten++] = Buffer.Dequeue();
            }

            if (bytesWritten == count)
                break;

            int next = source.ReadByte();
            if (next == -1)
            {
                // No more input: flush any remaining window as valid
                while (Window.Count > 0 && bytesWritten < count)
                {
                    outputBuffer[offset + bytesWritten++] = Window.Dequeue();
                }

                EndReached = true;
                break;
            }

            byte b = (byte)next;

            // KMP match logic
            while (MatchIndex > 0 && b != Pattern[MatchIndex])
            {
                MatchIndex = PrefixTable[MatchIndex - 1];
                // Move front of window to buffer (mismatch)
                Buffer.Enqueue(Window.Dequeue());
            }

            if (b == Pattern[MatchIndex])
            {
                Window.Enqueue(b);
                MatchIndex++;

                if (MatchIndex == Pattern.Length)
                {
                    // Full match — do not emit window
                    Window.Clear();
                    EndReached = true;
                    break;
                }
            }
            else
            {
                // Mismatch at beginning
                Buffer.Enqueue(b);
                MatchIndex = 0;
            }
        }

        return bytesWritten;
    }
}
