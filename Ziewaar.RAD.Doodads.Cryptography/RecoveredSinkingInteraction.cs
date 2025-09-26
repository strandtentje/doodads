namespace Ziewaar.RAD.Doodads.Cryptography;

public class RecoveredSinkingInteraction(IInteraction interaction, SinkNamingInteraction namingInteraction)
    : ISinkingInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public Encoding TextEncoding => namingInteraction.SinkingInteraction.TextEncoding;
    public Stream SinkBuffer => namingInteraction.SinkingInteraction.SinkBuffer;
    public string[] SinkContentTypePattern => namingInteraction.SinkingInteraction.SinkContentTypePattern;

    public string? SinkTrueContentType
    {
        get => namingInteraction.SinkingInteraction.SinkTrueContentType;
        set => namingInteraction.SinkingInteraction.SinkTrueContentType = value;
    }

    public long LastSinkChangeTimestamp
    {
        get => namingInteraction.SinkingInteraction.LastSinkChangeTimestamp;
        set => namingInteraction.SinkingInteraction.LastSinkChangeTimestamp = value;
    }

    public string Delimiter => namingInteraction.SinkingInteraction.Delimiter;

    public void SetContentLength64(long contentLength) =>
        namingInteraction.SinkingInteraction.SetContentLength64(contentLength);
}