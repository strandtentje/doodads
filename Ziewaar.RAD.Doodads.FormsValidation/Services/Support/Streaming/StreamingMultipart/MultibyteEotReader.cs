using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;
public class MultibyteEotReader(
    ICountingEnumerator<byte> byteSource, 
    byte[] eotMarker,
    long limit)
    : ICountingEnumerator<byte>
{
    public static MultibyteEotReader CreateForCrlf(ICountingEnumerator<byte> source, long limit = 1024) =>
        new(source, "\r\n"u8.ToArray(), limit);
    public static MultibyteEotReader CreateForCrLfDashDash(ICountingEnumerator<byte> source, long limit = int.MaxValue) =>
        new(source, "\r\n--"u8.ToArray(), limit);
    public static MultibyteEotReader CreateForAscii(ICountingEnumerator<byte> source, string asciiText, long limit = int.MaxValue) =>
        new(source, Encoding.ASCII.GetBytes(asciiText), limit);
    public byte Current { get; private set; }
    public byte[] FittingBuffer { get; private set; } = new byte [eotMarker.Length + 1];
    object? IEnumerator.Current => Current;
    public bool AtEnd { get; private set; }
    public long Cursor { get; private set; }
    private long Selection;
    public string? ErrorState { get; set; }
    public bool MoveNext()
    {
        if (Cursor >= limit)
        {
            ErrorState = "Reached limit";
            return false;
        }
        while (Selection < Cursor + eotMarker.Length)
        {
            if (!byteSource.MoveNext()) return false;
            FittingBuffer.SetCircular(Selection + 1, byte.MinValue).SetCircular(Selection++, byteSource.Current);
        }
        for (var i = Cursor; i < Selection; i++)
        {
            if (FittingBuffer.GetCircular(i) != eotMarker[i - Cursor])
            {
                Current = FittingBuffer.GetCircular(Cursor++);
                return true;
            }
        }
        AtEnd = true;
        return false;
    }
    public void ForSelection(Action<byte> callBack)
    {
        for (var i = Cursor; i < Selection; i++)
            callBack(FittingBuffer.GetCircular(i));
    }
    public void Reset()
    {
        AtEnd = false;
        Cursor = 0;
        Selection = 0;
        Array.Clear(FittingBuffer);
    }
    public void Dispose()
    {
    }
}