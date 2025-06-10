#pragma warning disable 67
#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
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
