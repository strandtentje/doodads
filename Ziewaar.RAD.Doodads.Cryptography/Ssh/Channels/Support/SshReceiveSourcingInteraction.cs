namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Channels.Support;
public class SshReceiveSourcingInteraction(IInteraction interaction, Stream receivingSshStream) : ISourcingInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public Stream SourceBuffer => receivingSshStream;
    public Encoding TextEncoding => NoEncoding.Instance;
    public string SourceContentTypePattern => "*/*";
    public long SourceContentLength => -1;
}