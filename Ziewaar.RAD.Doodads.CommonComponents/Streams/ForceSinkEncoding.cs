#nullable enable
using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Streams;
#pragma warning disable 67
[Category("Sourcing & Sinking")]
[Title("Force sink encoding")]
[Description("Overwrites the text encoding type of the sink; defaults to ascii")]
public class ForceSinkEncoding : IService
{
    [PrimarySetting("Name of the encoding to use as recognized by .net")]
    private readonly UpdatingPrimaryValue EncodingNameConstant = new();
    private Encoding CurrentEncoding = Encoding.ASCII;
    [EventOccasion("Here, we pretend we sink in the encoding")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when there was no sink or the encoding was not known")]
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