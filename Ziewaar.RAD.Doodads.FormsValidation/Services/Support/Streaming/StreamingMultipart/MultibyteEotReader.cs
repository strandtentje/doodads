using System.Reflection.Metadata;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class MultibyteEotReader
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

    public MultibyteEotReader(ICountingEnumerator<byte> byteSource, byte[] eotMarker, long limit)
    {
        this.ByteSource = byteSource;
        this.EotMarker = eotMarker;
        this.Limit = limit;

        int lookaheadBufferSize = 0b1000;

        for (; lookaheadBufferSize < 1 + eotMarker.Length * 8; lookaheadBufferSize *= 2) ;

        // this shifts the buffer size from 0b1000 to 0b10000 and 0b100000 and 0b1000000 and onwards until the eotmarker fits
        // we do this so that we end up with a mask like 0b0111, 0b01111, 0b0111111, 0b01111111 etc.
        // or we get an int overflow. maybe. shift harpens.

        this.LookaheadBuffer = new byte[lookaheadBufferSize];
        this.LookaheadBufferMask = lookaheadBufferSize - 1;
    }

    // private readonly Queue<byte> NonTerminatingBytes = new(eotMarker.Length + 1);
    public byte Current { get; private set; }
    public byte[] LookaheadBuffer { get; private set; }

    private readonly int LookaheadBufferMask;

    object? IEnumerator.Current => Current;
    public bool AtEnd { get; private set; }
    public long Cursor { get; private set; }
    private long LookaheadEndstop;
    private ICountingEnumerator<byte> ByteSource;
    private readonly byte[] EotMarker;
    private readonly long Limit;

    public string? ErrorState { get; set; }

    public bool MoveNext()
    {
        if (!IsBeforeLimit())
            return false;

        if (LookaheadEndstop < (Cursor + EotMarker.Length + 1))
        {
            while (LookaheadEndstop < (Cursor + EotMarker.Length * 8) && ByteSource.MoveNext())
            {
                LookaheadBuffer[(LookaheadEndstop++) & LookaheadBufferMask] = ByteSource.Current;
            }
        }

        var stopLookingHere = Math.Min(Cursor + EotMarker.Length, LookaheadEndstop);

        for (var detectionPosition = Cursor; detectionPosition < stopLookingHere; detectionPosition++)
        {
            var candidateEotByte = LookaheadBuffer[detectionPosition & LookaheadBufferMask];
            var expectedEotByte = EotMarker[detectionPosition - Cursor];
            if (candidateEotByte != expectedEotByte)
            {
                Current = LookaheadBuffer[Cursor++ & LookaheadBufferMask];
                return true;
            }
        }

        AtEnd = true;
        return false;
    }

    private bool IsBeforeLimit()
    {
        if (Cursor >= Limit)
        {
            ErrorState = "Reached limit";
            return false;
        }

        return true;
    }
    
    public void Reset()
    {
        AtEnd = false;
        Cursor = 0;
        LookaheadEndstop = 0;
        Array.Clear(LookaheadBuffer);
    }

    public void Dispose()
    {
    }
}