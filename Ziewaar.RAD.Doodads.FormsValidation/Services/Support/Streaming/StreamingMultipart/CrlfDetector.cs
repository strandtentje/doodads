using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class CrlfDetector(ICountingEnumerator<byte> byteSource) : ICountingEnumerator<byte>
{
    private bool atEnd;
    private long cursor;
    private byte current;
    private bool isCharacterSkipped;
    private byte skippedCharacter;
    private bool isTextOver;

    public bool AtEnd => atEnd;
    public long Cursor => cursor;
    public string? ErrorState { get; set; }
    public byte Current => current;
    object IEnumerator.Current => current;

    public bool MoveNext()
    {
        if (isTextOver)
        {
            return false;
        }
        if (isCharacterSkipped)
        {
            isCharacterSkipped = false;
            current = skippedCharacter;
            return true;
        }
        if (byteSource.MoveNext())
        {
            if (byteSource.Current == 13)
            {
                if (byteSource.MoveNext())
                {
                    if (byteSource.Current == 10)
                    {
                        atEnd = true;
                        return false;
                    }
                    else
                    {
                        isCharacterSkipped = true;
                        skippedCharacter = byteSource.Current;
                        this.current = 13;
                        return true;
                    }
                }
                else
                {
                    isTextOver = true;
                    this.current = 13;
                    return true;
                }
            }
            else
            {
                this.current = byteSource.Current;
                return true;
            }
        }
        else
        {
            atEnd = true;
            return false;
        }
    }

    public void Reset()
    {
        atEnd = false;
        cursor = 0;
        current = 0;
        isCharacterSkipped = false;
        skippedCharacter = 0; 
        isTextOver = false; 
    }
    public void Dispose()
    {

    }
}
