#nullable enable
using System.Text;
using Ziewaar.RAD.Doodads.EnumerableStreaming;

namespace Ziewaar.RAD.Doodads.CommonComponents.Streams;

[Category("Sourcing & Sinking")]
[Title("Peek at the Source")]
[Description("""
             Peek at the head chars of the provided source, stick them in the register, 
             and pass on the source without having consumed those head chars. Specify 
             the amount of chars to peek and what encoding to use for peeking. 
             """)]
public class SniffSourceText : IService
{
    [PrimarySetting("Amount of chars to peek")]
    private readonly UpdatingPrimaryValue SniffSizeConstant = new();

    [NamedSetting("encoding", "Encoding to use for peekig")]
    private readonly UpdatingKeyValue EncodingConstant = new("encoding");

    private int CurrentSniffSize = 1024;
    private Encoding CurrentEncoding = System.Text.Encoding.ASCII;

    [EventOccasion(
        "Untouched source comes out here, as well as the peeked text in register")]
    public event CallForInteraction? OnThen;

    [NeverHappens] public event CallForInteraction? OnElse;

    [EventOccasion(
        """
        When the sniff size is getting a little too exciting, or no sourcing 
        interaction was provided.
        """)]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, SniffSizeConstant).IsRereadRequired(() => 1024M,
                out decimal sniffSizeCandidate))
            this.CurrentSniffSize = (int)sniffSizeCandidate;
        if ((constants, EncodingConstant).IsRereadRequired(() => "ascii",
                out string? encodingCandidate))
            this.CurrentEncoding =
                Encoding.GetEncoding(encodingCandidate ?? "ascii");
        if (this.CurrentSniffSize < 16 || this.CurrentSniffSize > 1024 * 1024)
        {
            OnException?.Invoke(this,
                interaction.AppendRegister("Sniff size too big or too small"));
        }

        if (!interaction.TryGetClosest<ISourcingInteraction>(
                out var sourcingInteraction) ||
            sourcingInteraction == null)
        {
            OnException?.Invoke(this,
                interaction.AppendRegister("Sourcing interaction required"));
            return;
        }

        var sniffedStream =
            sourcingInteraction.SourceBuffer.TeeOff(out var head,
                CurrentSniffSize);
        var headText = CurrentEncoding.GetString(head.ToArray());
        var mimic = new MimicSourceInteraction(interaction, sourcingInteraction,
            sniffedStream);
        OnThen?.Invoke(this, mimic.AppendRegister(headText));
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}