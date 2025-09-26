using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Processes;
public class StandardInputInteraction(IInteraction interaction, Stream basestream, Encoding encoding = null) : ISinkingInteraction
{
    public Encoding TextEncoding => encoding ?? Console.InputEncoding ?? Encoding.Default;
    public Stream SinkBuffer => basestream;
    public string[] SinkContentTypePattern => ["*/*"];
    public string SinkTrueContentType { get; set; }
    public long LastSinkChangeTimestamp { get; set; } = DateTime.Now.ToBinary();
    public string Delimiter => "";
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public void SetContentLength64(long contentLength) { }
}