using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Streams;

public class RecoveredSourcingInteraction(IInteraction interaction, SourceNamingInteraction namingInteraction)
    : ISourcingInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public Stream SourceBuffer => namingInteraction.SourcingInteraction.SourceBuffer;
    public Encoding TextEncoding => namingInteraction.SourcingInteraction.TextEncoding;
    public string SourceContentTypePattern => namingInteraction.SourcingInteraction.SourceContentTypePattern;
    public long SourceContentLength => namingInteraction.SourcingInteraction.SourceContentLength;
}