using System.Reflection.Metadata;
using System.Runtime.InteropServices.JavaScript;
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

    public static MultibyteEotReader
        CreateForCrLfDashDash(ICountingEnumerator<byte> source, long limit = int.MaxValue) =>
        new(source, "\r\n--"u8.ToArray(), limit);

    public static MultibyteEotReader CreateForAscii(ICountingEnumerator<byte> source, string asciiText,
        long limit = int.MaxValue) =>
        new(source, Encoding.ASCII.GetBytes(asciiText), limit);

    // private readonly Queue<byte> NonTerminatingBytes = new(eotMarker.Length + 1);
    public byte Current { get; private set; }
    public byte[] DetectionBuffer { get; private set; } = new byte [eotMarker.Length + 1];
    object? IEnumerator.Current => Current;
    public bool AtEnd { get; private set; }
    public long Cursor { get; private set; }
    private long DetectionCursor;
    public string? ErrorState { get; set; }

    public bool MoveNext()
    {
        if (!IsBeforeLimit())
            return false;

        RefillDetectionBuffer();

        for (var detectionPosition = Cursor; detectionPosition < DetectionCursor; detectionPosition++)
            if (HasFoundNewNonTerminatingByte(detectionPosition))
                return true;

        return TerminationSequenceDetected();
    }

    private bool IsBeforeLimit()
    {
        if (Cursor >= limit)
        {
            ErrorState = "Reached limit";
            return false;
        }

        return true;
    }

    private void RefillDetectionBuffer()
    {
        while (DetectionCursor < Cursor + eotMarker.Length)
        {
            if (!byteSource.MoveNext())
                break;
            DetectionBuffer.SetCircular(DetectionCursor + 1, byte.MinValue)
                .SetCircular(DetectionCursor++, byteSource.Current);
        }
    }

    private bool HasFoundNewNonTerminatingByte(long bytePosition)
    {
        var candidateEotByte = DetectionBuffer.GetCircular(bytePosition);
        if (candidateEotByte != eotMarker[bytePosition - Cursor])
        {
            Current = DetectionBuffer.GetCircular(Cursor++);
            return true;
        }

        return false;
    }

    private bool TerminationSequenceDetected()
    {
        AtEnd = true;
        return false;
    }

    public void ForSelection(Action<byte> callBack)
    {
        for (var i = Cursor; i < DetectionCursor; i++)
            callBack(DetectionBuffer.GetCircular(i));
    }

    public void Reset()
    {
        AtEnd = false;
        Cursor = 0;
        DetectionCursor = 0;
        Array.Clear(DetectionBuffer);
    }

    public void Dispose()
    {
    }
}