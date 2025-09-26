namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Channels.Support;
public class SshReceiveSinkingInteraction(IInteraction interaction, Stream sendingSshStream) : ISinkingInteraction
{
    public IInteraction Stack => interaction.Stack;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public Encoding TextEncoding => NoEncoding.Instance;
    public Stream SinkBuffer => sendingSshStream;
    public string[] SinkContentTypePattern => ["*/*"];
    public string? SinkTrueContentType { get; set; } = "application/octet-stream";
    public long LastSinkChangeTimestamp { get; set; } =
        GlobalStopwatch.Instance.ElapsedTicks;
    public string Delimiter => "";
    public void SetContentLength64(long contentLength)
    {
    }
}