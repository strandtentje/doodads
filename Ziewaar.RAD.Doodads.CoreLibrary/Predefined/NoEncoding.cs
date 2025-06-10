using System.Text;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
#nullable enable

public class NoEncoding : Encoding
{
    public override int GetByteCount(char[] chars, int index, int count) =>
        throw new InvalidOperationException("Cannot write text to binary encoding stream");

    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) =>
        throw new InvalidOperationException("Cannot write text to binary encoding stream");

    public override int GetCharCount(byte[] bytes, int index, int count) =>
        throw new InvalidOperationException("Cannot write text to binary encoding stream");

    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) =>
        throw new InvalidOperationException("Cannot write text to binary encoding stream");

    public override int GetMaxByteCount(int charCount) =>
        throw new InvalidOperationException("Cannot write text to binary encoding stream");

    public override int GetMaxCharCount(int byteCount) =>
        throw new InvalidOperationException("Cannot write text to binary encoding stream");
}
