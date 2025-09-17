using System.Reflection.Metadata;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class MultibyteEotReader(
    ICountingEnumerator<byte> byteSource,
    byte[] eotMarker)
    : ICountingEnumerator<byte>
{
    public static MultibyteEotReader CreateForCrlf(ICountingEnumerator<byte> source) =>
        new(source, "\r\n"u8.ToArray());

    public static MultibyteEotReader
        CreateForCrLfDashDash(ICountingEnumerator<byte> source) =>
        new(source, "\r\n--"u8.ToArray());

    public static MultibyteEotReader CreateForAscii(ICountingEnumerator<byte> source, string asciiText) =>
        new(source, Encoding.ASCII.GetBytes(asciiText));

    // private readonly Queue<byte> NonTerminatingBytes = new(eotMarker.Length + 1);
    public byte Current => _current;
    private byte _current;
    private byte[] _detectionBuffer = new byte[eotMarker.Length + 1];
    object? IEnumerator.Current => Current;
    public bool AtEnd { get; private set; }
    private long _exposedCursor;
    public long Cursor => _exposedCursor;
    private long _realCursor;
    private long _detectionCursor;
    public string? ErrorState { get; set; }

    private byte[] _prefetchBuffer = new byte[1024];
    private int _prefetchCursor = 0;
    private int _prefetchEndstop = 0;
    private bool _endInPrefetch = false;
    private long _roomToDetect;
    private long _positionToDetect;
    public bool MoveNext()
    {
        if (!_endInPrefetch && _prefetchCursor >= _prefetchEndstop)
        {
            _prefetchCursor = 0;
            _prefetchEndstop = 0;

            _roomToDetect = _realCursor + eotMarker.Length;
            while (_detectionCursor < _roomToDetect - 1 && byteSource.MoveNext())
                _detectionBuffer[_detectionCursor++ % _detectionBuffer.Length] = byteSource.Current;

            while (_prefetchEndstop < _prefetchBuffer.Length)
            {
                if (byteSource.MoveNext())
                {
                    _detectionBuffer[_detectionCursor++ % _detectionBuffer.Length] = byteSource.Current;
                    for (_positionToDetect = _realCursor; _positionToDetect < _detectionCursor; _positionToDetect++)
                    {
                        if (_detectionBuffer[_positionToDetect % _detectionBuffer.Length] != eotMarker[_positionToDetect - _realCursor])
                        {
                            _prefetchBuffer[_prefetchEndstop++] = _detectionBuffer[_realCursor++ % _detectionBuffer.Length];
                            goto nonMatching;
                        }
                    }
                }
                _endInPrefetch = true;
                break;
                nonMatching:;
            }
        }

        if (_prefetchCursor < _prefetchEndstop)
        {
            _current = _prefetchBuffer[_prefetchCursor++];
            _exposedCursor++;
            return true;
        }

        AtEnd = true;
        return false;
    }

    public void Reset()
    {
        AtEnd = false;
        _exposedCursor = 0;
        _realCursor = 0;
        _detectionCursor = 0;
        _prefetchCursor = 0;
        _prefetchEndstop = 0;
        _endInPrefetch = false;
        Array.Clear(_detectionBuffer);
    }

    public void Dispose()
    {
    }
}