namespace Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;
public class UrlEncodedTokenReader(
    ICountingEnumerator<byte> byteSource,
    long keyLengthLimit = 200) : ICountingEnumerator<KeyValuePair<string, UnicodeConvertingReader>>
{
    private const byte EQUALS = 0x3d, AMPERSAND = 0x26;
    public KeyValuePair<string, UnicodeConvertingReader> Current { get; private set; }
    public bool AtEnd { get; }
    public long Cursor { get; }
    public string? ErrorState { get => byteSource.ErrorState; set =>  byteSource.ErrorState = value; }
    
    public bool MoveNext()
    {
        if (Current.Value.CanContinue())
        {
            this.ErrorState ??= "Previous pair wasn't fully read.";
            return false;
        }
        if (!byteSource.CanContinue())
            return false;
        if (byteSource.Cursor > 0)
        {
            if (!byteSource.Expect(AMPERSAND))
            {
                byteSource.ErrorState = "Expected ampersand midway in form";
                return false;
            }
        }
        
        var keyBytesReader = new UrlEncodedValueBytesReader(byteSource, keyLengthLimit);
        var keyCharsReader = new UnicodeConvertingReader(keyBytesReader, keyLengthLimit);

        if (keyCharsReader.TryRenderToString(out string eqKey))
        {
            if (byteSource.CanContinue())
            {
                switch (byteSource.Current)
                {
                    case EQUALS:
                        Current = new(eqKey, new UnicodeConvertingReader(new UrlEncodedValueBytesReader(byteSource)));
                        break;
                    case AMPERSAND:
                        Current = new(eqKey, UnicodeConvertingReader.Empty);
                        break;
                    default:
                        byteSource.ErrorState = $"Expected = or & at {byteSource.Cursor}";
                        break;
                }
            }
            else 
            {
                Current = new(eqKey, UnicodeConvertingReader.Empty);
            }
        }
        return byteSource.ErrorState == null;
    }
    public void Reset() => throw new InvalidOperationException("Can't reset.");
    public void Dispose()
    {
        if (!AtEnd || this.ErrorState != null)
            throw new Exception($"Disposing unfinished object; {this.ErrorState}");
    }
    
    object? IEnumerator.Current => Current;
}

