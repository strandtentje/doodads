#nullable enable
using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Streams;
#pragma warning disable 67
[Category("Sourcing & Sinking")]
[Title("Force sink encoding")]
[Description("""
             Overwrites the text encoding type of the sink; defaults to ascii
             This is useful when working with sinks that are presumed to be binary,
             ie the ones coming from a TCP connection or a SinkToFile
             """)]
public class ForceSinkEncoding : IService
{
    [PrimarySetting("Encoding name as accepted by .net (ie. ascii or utf-8)")]
    private readonly UpdatingPrimaryValue EncodingNameConstant = new();
    private Encoding CurrentEncoding = Encoding.ASCII;
    [EventOccasion("Existing sink is propagated here, but signalling that text encoding is as configured here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no sink was present to override encoding on.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, EncodingNameConstant).IsRereadRequired(out string? encodingNameCandidate) &&
            encodingNameCandidate != null)
            this.CurrentEncoding = Encoding.GetEncoding(encodingNameCandidate);
        if (!interaction.TryGetClosest<ISinkingInteraction>(out ISinkingInteraction? sinkingInteraction)
            || sinkingInteraction == null)
        {
            OnException?.Invoke(this, interaction.AppendRegister("sink required for this"));
            return;
        }

        OnThen?.Invoke(this,
            new MimicSinkInteraction(interaction, sinkingInteraction, this.CurrentEncoding,
                sinkingInteraction.SinkBuffer));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}