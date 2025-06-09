using System.Text;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

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

public class TextSinkingInteraction(
    IInteraction parent,
    string[]? pattern = null,
    string delimiter = "",
    Encoding? textEncoding = null,
    object? register = null,
    IReadOnlyDictionary<string, object>? memory = null)
    : ISinkingInteraction
{
    public IInteraction Stack { get; } = parent;
    public object Register { get; } = register ?? parent.Register;
    public IReadOnlyDictionary<string, object> Memory { get; } = memory ?? parent.Memory;
    public Encoding TextEncoding { get; } = textEncoding ?? Encoding.Unicode;
    public Stream SinkBuffer { get; } = new MemoryStream();
    public string[] SinkContentTypePattern { get; } = pattern ?? ["*/*"];
    public string? SinkTrueContentType { get; set; }
    public long LastSinkChangeTimestamp { get; set; }
    public string Delimiter { get; } = delimiter;

    public static TextSinkingInteraction CreateIntermediateFor(ISinkingInteraction original, IInteraction offset,
        object? register = null, IReadOnlyDictionary<string, object>? memory = null) =>
        new(offset, original.SinkContentTypePattern, original.Delimiter,
            original.TextEncoding, register, memory);
}

public static class SinkingInteractionExtensions
{
    public static StreamWriter GetWriter<TInteraction>(this TInteraction interaction, string? contentType = null)
        where TInteraction : ISinkingInteraction
    {
        contentType ??= "text/*";
        ContentTypeMatcher.Assert(interaction.SinkContentTypePattern, contentType);
        interaction.LastSinkChangeTimestamp = GlobalStopwatch.Instance.ElapsedTicks;
        return new(interaction.SinkBuffer, interaction.TextEncoding, bufferSize: -1, leaveOpen: true)
        {
            AutoFlush = true,
        };
    }
    public static TInteraction Write<TInteraction>(this TInteraction interaction, string text, string? contentType = null)
        where TInteraction : ISinkingInteraction
    {
        using var wr = interaction.GetWriter(contentType);
        wr.Write(text);
        wr.Flush();
        return interaction;
    }
    public static TInteraction WriteSegment<TInteraction>(this TInteraction interaction, string text, string? contentType = null)
        where TInteraction : ISinkingInteraction
    {
        using var wr = interaction.GetWriter(contentType);
        wr.Write(text);
        wr.Write(interaction.Delimiter);
        wr.Flush();
        return interaction;
    }

    public static TInteraction CopyTextFrom<TInteraction>(this TInteraction target,
        TextSinkingInteraction source)
        where TInteraction : ISinkingInteraction
    {
        target.CopyTextFrom(source.GetDisposingSinkReader(), source.SinkTrueContentType);
        return target;
    }
    public static TInteraction CopyTextFrom<TInteraction>(this TInteraction interaction, StreamReader reader,
        string? contentType = null)
        where TInteraction : ISinkingInteraction
    {
        contentType ??= "text/*";
        ContentTypeMatcher.Assert(interaction.SinkContentTypePattern, contentType);
        using StreamWriter wr = new(interaction.SinkBuffer, interaction.TextEncoding, bufferSize: -1, true);
        int bufferSize = 4096;
        try
        {
            bufferSize = (int)Math.Min(bufferSize, reader.BaseStream.Length);
        }
        catch (Exception)
        {
            // we have a good default buffer size.
        }
        var copyBuffer = new char[bufferSize];
        int trueReadCount = 0;
        while (!reader.EndOfStream)
        {
            trueReadCount = reader.Read(copyBuffer, 0, bufferSize);
            wr.Write(copyBuffer, 0, trueReadCount);
        }
        return interaction;
    }
    public static StreamReader GetDisposingSinkReader<TInteraction>(this TInteraction interaction)
        where TInteraction : ISinkingInteraction =>
        new(interaction.SinkBuffer, interaction.TextEncoding, detectEncodingFromByteOrderMarks: false,
            bufferSize: -1, leaveOpen: false);
}

public class ContentTypeMismatchException(string expected, string actual)
    : Exception($"Expected content type {expected} but got {actual}");