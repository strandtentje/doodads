namespace Ziewaar.RAD.Doodads.EnumerableStreaming;
#pragma warning disable 67
public static class StreamExtensions
{
    /// <summary>
    /// Reads up to <paramref name="count"/> bytes into a shared buffer.
    /// Returns a ConcatenatedStream of (head + remainder), while also
    /// handing out a separate MemoryStream head for inspection.
    /// </summary>
    public static ConcatenatedStream TeeOff(
        this Stream input, out MemoryStream head, int count)
    {
        byte[] headBytes = new byte[count];
        int read = 0;
        try
        {
            input.ReadTimeout = 100;
            while (read < count)
            {
                try
                {
                    int r = input.Read(headBytes, read, count - read);
                    if (r == 0) break; // EOF
                    read += r;
                }
                catch (Exception)
                {
                    break;
                }
            }

            head = new MemoryStream(headBytes, 0, read, writable: false);
            var replay = new MemoryStream(headBytes, 0, read, writable: false);

            return new ConcatenatedStream(replay, input);
        }
        finally
        {
            input.ReadTimeout = -1;
        }
    }
}